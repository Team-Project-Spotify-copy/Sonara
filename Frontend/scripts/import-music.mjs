#!/usr/bin/env node
/**
 * Development-only bulk importer.
 *
 * It adds nothing to the architecture: every file is POSTed to the existing
 * admin endpoints, so uploads go through the existing BlobService into the
 * existing Azure containers and the rows are created by the existing
 * AdminMusicService. No audio ever touches the repository.
 *
 * Two sources:
 *
 *   folder   scan a local directory of audio files (the original mode)
 *   jamendo  pull tracks from the Jamendo v3.0 API, with their artist, album,
 *            genres and cover art
 *
 * Endpoints used:
 *   POST /api/auth/login            -> obtain a JWT (needs an Admin account)
 *   GET  /api/tracks                -> discover existing artists / titles
 *   POST /api/admin/artists/resolve -> get-or-create an Artist (idempotent)
 *   POST /api/admin/albums/resolve  -> get-or-create an Album  (idempotent)
 *   POST /api/admin/tracks          -> multipart upload + Track creation
 *
 * Usage:
 *   node scripts/import-music.mjs --dir "D:\\SonaraMusic" --email you@example.com --password ****
 *   node scripts/import-music.mjs --source jamendo --limit 500 --tags "rock,electronic"
 *
 * Credentials come from the environment so nothing is committed:
 *   JAMENDO_CLIENT_ID      Jamendo application client id (jamendo mode)
 *   SONARA_API             API base, default http://localhost:5094/api
 *   SONARA_ADMIN_TOKEN     an existing Admin JWT, or:
 *   SONARA_ADMIN_EMAIL     Admin account e-mail
 *   SONARA_ADMIN_PASSWORD  Admin account password
 *
 * Load them from an untracked file with Node's own flag:
 *   node --env-file=.env scripts/import-music.mjs --source jamendo --limit 500
 *
 * Options (CLI wins over the environment):
 *   --source <folder|jamendo>  defaults to folder when --dir is given
 *   --api <url>                API base
 *   --token <jwt>              use an existing Admin token instead of logging in
 *   --email/--password         Admin credentials (Development reCAPTCHA bypass)
 *   --concurrency <n>          parallel track uploads, default 4
 *   --dry-run                  show what would happen, upload nothing
 *
 * folder mode:
 *   --dir <path>          folder to scan (required)
 *   --artist <name>       reuse the existing artist with this name
 *   --artist-id <guid>    reuse this artist id directly
 *   --album-id <guid>     attach every imported track to this album
 *
 * jamendo mode:
 *   --client-id <id>      Jamendo client id (else JAMENDO_CLIENT_ID)
 *   --limit <n>           total tracks to pull, default 200
 *   --offset <n>          starting offset, default 0
 *   --page-size <n>       results per request, 1..200, default 200
 *   --tags <list>         Jamendo tag filter, e.g. "rock,electronic"
 *   --search <text>       full-text filter
 *   --artist-name <name>  restrict to one Jamendo artist
 *   --order <field>       Jamendo ordering, default popularity_total
 *
 * Only tracks with audiodownload_allowed = true are imported; the rest are
 * counted as skipped so the licence is never bypassed.
 */

import { readdir, readFile, stat } from "node:fs/promises";
import { basename, extname, join } from "node:path";
import { parseFile } from "music-metadata";

const AUDIO_EXTENSIONS = new Set([".mp3", ".m4a", ".mp4", ".aac", ".ogg", ".oga", ".opus", ".wav", ".flac", ".webm"]);

const MIME = {
  ".mp3": "audio/mpeg",
  ".m4a": "audio/mp4",
  ".mp4": "audio/mp4",
  ".aac": "audio/mp4",
  ".ogg": "audio/ogg",
  ".oga": "audio/ogg",
  ".opus": "audio/opus",
  ".wav": "audio/wav",
  ".flac": "audio/flac",
  ".webm": "audio/webm",
};

const JAMENDO_API = "https://api.jamendo.com/v3.0";
const JAMENDO_MAX_PAGE = 200;
const DOWNLOAD_TIMEOUT_MS = 120_000;

// The Jamendo API intermittently answers with status "success" and an empty
// result set. Treating the first empty page as the end of the catalog silently
// truncates a large import, so pages and downloads are retried before a page is
// accepted as final.
const RETRIES = 4;
const RETRY_BASE_MS = 600;

function parseArgs(argv) {
  const args = {};
  for (let i = 2; i < argv.length; i += 1) {
    const key = argv[i];
    if (!key.startsWith("--")) continue;
    const name = key.slice(2);
    const next = argv[i + 1];
    if (next === undefined || next.startsWith("--")) {
      args[name] = true;
    } else {
      args[name] = next;
      i += 1;
    }
  }
  return args;
}

function fail(message) {
  console.error(`\n  ✗ ${message}\n`);
  process.exit(1);
}

/** CLI flag, then environment, then default. */
function option(args, flag, envName, fallback = null) {
  if (typeof args[flag] === "string") return args[flag];
  if (envName && process.env[envName]) return process.env[envName];
  return fallback;
}

function toInt(value, fallback) {
  const n = Number.parseInt(value, 10);
  return Number.isFinite(n) ? n : fallback;
}

/** Jamendo returns HTML-escaped names ("John Dada &amp; the Weathermen"). */
function decodeEntities(value) {
  if (typeof value !== "string") return value;
  return value
    .replace(/&#(\d+);/g, (_, code) => String.fromCodePoint(Number(code)))
    .replace(/&#x([0-9a-f]+);/gi, (_, code) => String.fromCodePoint(Number.parseInt(code, 16)))
    .replace(/&quot;/g, '"')
    .replace(/&apos;/g, "'")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&nbsp;/g, " ")
    .replace(/&amp;/g, "&")
    .trim();
}

/** Titles differ only by casing and spacing between runs; compare on that. */
function normalizeTitle(value) {
  return (value ?? "").trim().toLowerCase().replace(/\s+/g, " ");
}

/**
 * Shares one in-flight promise per key. Without it, concurrent workers hitting
 * the same new artist would each see "not found" and create a duplicate before
 * the first insert lands.
 */
function memoize(loader) {
  const cache = new Map();
  return (key, ...rest) => {
    if (!cache.has(key)) cache.set(key, loader(key, ...rest));
    return cache.get(key);
  };
}

/** Bounded worker pool - keeps a large run from opening thousands of sockets. */
async function runPool(items, concurrency, worker) {
  let cursor = 0;
  const size = Math.max(1, Math.min(concurrency, items.length || 1));

  const runners = Array.from({ length: size }, async () => {
    for (;;) {
      const index = cursor;
      cursor += 1;
      if (index >= items.length) return;
      await worker(items[index], index);
    }
  });

  await Promise.all(runners);
}

async function login(api, email, password) {
  const res = await fetch(`${api}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    // RecaptchaServices accepts this literal only in the Development environment.
    body: JSON.stringify({ email, password, token: "dev-dummy-token" }),
  });
  if (!res.ok) fail(`Login failed (HTTP ${res.status}). Check the credentials and that the API runs in Development.`);
  const data = await res.json();
  if (!data.accessToken) fail("Login returned no accessToken.");
  return data.accessToken;
}

/** The catalog has no artist listing endpoint, so existing artists are
 *  discovered from the tracks already in the catalog. */
async function loadArtists(api, token) {
  const res = await fetch(`${api}/tracks?page=1&pageSize=100`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) fail(`Could not read the catalog (HTTP ${res.status}).`);
  const body = await res.json();
  const map = new Map();
  for (const t of body.items ?? []) {
    if (t.artistId && !map.has(t.artistName?.toLowerCase())) {
      map.set((t.artistName ?? "").toLowerCase(), { id: t.artistId, name: t.artistName });
    }
  }
  return map;
}

async function listAudioFiles(dir) {
  let entries;
  try {
    entries = await readdir(dir);
  } catch {
    fail(`Cannot read folder: ${dir}`);
  }
  const files = [];
  for (const name of entries) {
    const full = join(dir, name);
    const info = await stat(full);
    if (info.isFile() && AUDIO_EXTENSIONS.has(extname(name).toLowerCase())) files.push(full);
  }
  return files.sort();
}

// ---------------------------------------------------------------- catalog API

function createCatalog(api, token) {
  const auth = { Authorization: `Bearer ${token}` };

  /** Every title the catalog already holds for one artist, for dedupe. */
  const titlesForArtist = memoize(async (artistId) => {
    const seen = new Set();
    for (let page = 1; page <= 50; page += 1) {
      const res = await fetch(`${api}/tracks?page=${page}&pageSize=200&artistId=${artistId}`, {
        headers: auth,
      });
      if (!res.ok) break;
      const body = await res.json();
      for (const t of body.items ?? []) seen.add(normalizeTitle(t.title));
      if (!body.hasNextPage) break;
    }
    return seen;
  });

  const resolveArtist = memoize(async (_key, { name, bio, avatarUrl }) => {
    const form = new FormData();
    form.append("Name", name);
    if (bio) form.append("Bio", bio);
    if (avatarUrl) form.append("AvatarUrl", avatarUrl);

    const res = await fetch(`${api}/admin/artists/resolve`, { method: "POST", headers: auth, body: form });
    if (!res.ok) throw new Error(`artist "${name}": HTTP ${res.status} ${(await res.text()).slice(0, 120)}`);
    return res.json();
  });

  const resolveAlbum = memoize(async (_key, { title, artistId, releaseDate, cover }) => {
    const form = new FormData();
    form.append("Title", title);
    form.append("ArtistId", artistId);
    if (releaseDate) form.append("ReleaseDate", releaseDate);
    if (cover) form.append("CoverImage", cover.blob, cover.filename);

    const res = await fetch(`${api}/admin/albums/resolve`, { method: "POST", headers: auth, body: form });
    if (!res.ok) throw new Error(`album "${title}": HTTP ${res.status} ${(await res.text()).slice(0, 120)}`);
    return res.json();
  });

  async function createTrack({ title, durationSeconds, artistId, albumId, genres, audio }) {
    const form = new FormData();
    form.append("Title", title);
    form.append("DurationSeconds", String(durationSeconds));
    form.append("ArtistId", artistId);
    if (albumId) form.append("AlbumId", albumId);
    for (const genre of genres ?? []) form.append("GenreNames", genre);
    form.append("AudioFile", audio.blob, audio.filename);

    const res = await fetch(`${api}/admin/tracks`, { method: "POST", headers: auth, body: form });
    if (!res.ok) throw new Error(`HTTP ${res.status} ${(await res.text()).slice(0, 160)}`);
    return res.json();
  }

  return { titlesForArtist, resolveArtist, resolveAlbum, createTrack };
}

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

/** Retries transient failures with a widening delay. */
async function withRetry(label, attempt) {
  let lastError;
  for (let i = 0; i < RETRIES; i += 1) {
    try {
      return await attempt();
    } catch (err) {
      lastError = err;
      if (i < RETRIES - 1) await sleep(RETRY_BASE_MS * 2 ** i);
    }
  }
  throw new Error(`${label}: ${lastError?.message ?? lastError}`);
}

/**
 * `contentType` is forced rather than taken from the response: Jamendo serves
 * audio with a text/html header, and BlobService copies whatever it is given
 * onto the stored blob.
 */
async function download(url, { contentType, filename }) {
  return withRetry(`download ${filename}`, async () => {
    const res = await fetch(url, { signal: AbortSignal.timeout(DOWNLOAD_TIMEOUT_MS), redirect: "follow" });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);

    const bytes = Buffer.from(await res.arrayBuffer());
    if (bytes.length === 0) throw new Error("empty body");

    return { blob: new Blob([bytes], { type: contentType }), filename, bytes: bytes.length };
  });
}

// -------------------------------------------------------------- jamendo fetch

async function fetchJamendoPage(clientId, { limit, offset, tags, search, artistName, order }) {
  const params = new URLSearchParams({
    client_id: clientId,
    format: "json",
    limit: String(limit),
    offset: String(offset),
    include: "musicinfo",
    audioformat: "mp32",
    order,
  });
  if (tags) params.set("tags", tags);
  if (search) params.set("search", search);
  if (artistName) params.set("artist_name", artistName);

  return withRetry(`jamendo offset ${offset}`, async () => {
    const res = await fetch(`${JAMENDO_API}/tracks/?${params}`, {
      signal: AbortSignal.timeout(DOWNLOAD_TIMEOUT_MS),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);

    const body = await res.json();
    const status = body?.headers?.status;
    if (status !== "success") {
      // A rejected key or a bad filter is permanent - do not burn retries on it.
      fail(`Jamendo rejected the request: ${body?.headers?.error_message ?? status ?? "unknown error"}`);
    }
    return body.results ?? [];
  });
}

async function collectJamendoTracks(clientId, opts) {
  const wanted = opts.limit;
  const collected = [];
  const seenIds = new Set();
  let offset = opts.offset;

  while (collected.length < wanted) {
    const pageSize = Math.min(opts.pageSize, wanted - collected.length);

    // An empty page is usually the API being flaky rather than the end of the
    // catalog, so confirm it before giving up on the remaining tracks.
    let page = [];
    for (let attempt = 0; attempt < RETRIES; attempt += 1) {
      page = await fetchJamendoPage(clientId, { ...opts, limit: pageSize, offset });
      if (page.length > 0) break;
      if (attempt < RETRIES - 1) {
        console.log(`  … empty page at offset ${offset}, retrying`);
        await sleep(RETRY_BASE_MS * 2 ** attempt);
      }
    }

    if (page.length === 0) {
      console.log(`  … no more results at offset ${offset}`);
      break;
    }

    for (const track of page) {
      const id = String(track.id ?? "");
      if (id && seenIds.has(id)) continue;
      if (id) seenIds.add(id);
      collected.push(track);
    }

    offset += page.length;
    console.log(`  … fetched ${collected.length}/${wanted}`);

    if (page.length < pageSize) break;
  }

  return collected.slice(0, wanted);
}

// ---------------------------------------------------------------------- modes

async function importFolder(args, api, token, dryRun) {
  const dir = args.dir;
  const files = await listAudioFiles(dir);
  if (files.length === 0) fail(`No audio files found in ${dir}`);
  console.log(`\n  Found ${files.length} audio file(s) in ${dir}`);

  const artists = dryRun && !token ? new Map() : await loadArtists(api, token);
  console.log(`  Known artists in catalog: ${artists.size}`);

  let artistId = typeof args["artist-id"] === "string" ? args["artist-id"] : null;
  if (!artistId && typeof args.artist === "string") {
    const found = artists.get(args.artist.toLowerCase());
    if (!found) {
      fail(
        `No existing artist named "${args.artist}". Known: ${[...artists.values()].map((a) => a.name).join(", ") || "(none)"}\n` +
          "    Pass --artist-id, or import through --source jamendo which creates artists.",
      );
    }
    artistId = found.id;
  }
  if (!artistId && artists.size > 0) {
    artistId = [...artists.values()][0].id;
    console.log(`  No --artist given, defaulting to existing artist "${[...artists.values()][0].name}".`);
  }
  if (!artistId && !dryRun) {
    fail("No artist available. Pass --artist-id.");
  }

  const albumId = typeof args["album-id"] === "string" ? args["album-id"] : null;
  const catalog = dryRun ? null : createCatalog(api, token);

  let ok = 0;
  let failed = 0;

  for (const file of files) {
    const name = basename(file);
    const ext = extname(file).toLowerCase();

    let title = basename(file, ext);
    let durationSeconds = 0;
    try {
      const meta = await parseFile(file);
      durationSeconds = Math.max(0.1, Math.round((meta.format.duration ?? 0) * 1000) / 1000);
      if (meta.common?.title) title = meta.common.title;
    } catch {
      // Fall back to the filename; duration is required by CreateTrackDto.
    }

    if (!durationSeconds || durationSeconds < 0.1) {
      console.log(`  – ${name}: skipped (could not read duration)`);
      failed += 1;
      continue;
    }

    if (dryRun) {
      console.log(`  · ${name} -> "${title}" (${durationSeconds.toFixed(1)}s)`);
      ok += 1;
      continue;
    }

    try {
      const bytes = await readFile(file);
      const id = await catalog.createTrack({
        title,
        durationSeconds,
        artistId,
        albumId,
        genres: [],
        audio: {
          blob: new Blob([bytes], { type: MIME[ext] ?? "application/octet-stream" }),
          filename: name,
        },
      });
      console.log(`  ✓ ${name} -> "${title}"  ${id}`);
      ok += 1;
    } catch (err) {
      console.log(`  ✗ ${name}: ${err.message}`);
      failed += 1;
    }
  }

  return { ok, skipped: 0, failed };
}

async function importJamendo(args, api, token, dryRun) {
  const clientId = option(args, "client-id", "JAMENDO_CLIENT_ID");
  if (!clientId) {
    fail("Missing Jamendo client id. Set JAMENDO_CLIENT_ID or pass --client-id.");
  }

  const opts = {
    limit: toInt(option(args, "limit", null, "200"), 200),
    offset: toInt(option(args, "offset", null, "0"), 0),
    pageSize: Math.min(JAMENDO_MAX_PAGE, toInt(option(args, "page-size", null, "200"), JAMENDO_MAX_PAGE)),
    tags: option(args, "tags", null),
    search: option(args, "search", null),
    artistName: option(args, "artist-name", null),
    order: option(args, "order", null, "popularity_total"),
  };

  console.log(`\n  Jamendo: pulling up to ${opts.limit} track(s) from offset ${opts.offset}`);
  const raw = await collectJamendoTracks(clientId, opts);
  if (raw.length === 0) fail("Jamendo returned no tracks for those filters.");

  // Only tracks the licence allows to be downloaded are re-hosted.
  const tracks = raw
    .filter((t) => t.audiodownload_allowed === true && Boolean(t.audio))
    .map((t) => ({
      ...t,
      name: decodeEntities(t.name),
      artist_name: decodeEntities(t.artist_name),
      album_name: decodeEntities(t.album_name),
    }));

  const licenceSkipped = raw.length - tracks.length;
  console.log(`  Downloadable: ${tracks.length}${licenceSkipped ? ` (${licenceSkipped} skipped, licence disallows download)` : ""}`);

  if (dryRun) {
    const artists = new Set(tracks.map((t) => normalizeTitle(t.artist_name)));
    const albums = new Set(tracks.filter((t) => t.album_name).map((t) => normalizeTitle(t.album_name)));
    const singles = tracks.filter((t) => !t.album_name).length;

    for (const t of tracks.slice(0, 20)) {
      const genres = t.musicinfo?.tags?.genres ?? [];
      console.log(`  · ${t.artist_name} — ${t.name} [${t.album_name || "single"}] ${t.duration}s ${genres.join("/")}`);
    }
    if (tracks.length > 20) console.log(`  · … and ${tracks.length - 20} more`);
    console.log(`
  Would touch ${artists.size} artist(s), ${albums.size} album(s), ${singles} single(s).`);
    return { ok: tracks.length, skipped: licenceSkipped, failed: 0 };
  }

  const catalog = createCatalog(api, token);
  const concurrency = toInt(option(args, "concurrency", null, "4"), 4);
  console.log(`  Uploading with concurrency ${concurrency}…\n`);

  let ok = 0;
  let skipped = licenceSkipped;
  let failed = 0;
  let done = 0;

  await runPool(tracks, concurrency, async (track) => {
    const position = (done += 1);
    const label = `[${position}/${tracks.length}] ${track.artist_name} — ${track.name}`;

    try {
      const durationSeconds = Number(track.duration);
      if (!Number.isFinite(durationSeconds) || durationSeconds < 0.1) {
        console.log(`  – ${label}: skipped (no duration)`);
        skipped += 1;
        return;
      }

      const artist = await catalog.resolveArtist(normalizeTitle(track.artist_name), {
        name: track.artist_name,
        // Gives tracks without an album something to fall back to for artwork.
        avatarUrl: track.image || null,
      });

      const existing = await catalog.titlesForArtist(artist.id);
      if (existing.has(normalizeTitle(track.name))) {
        console.log(`  – ${label}: already in catalog`);
        skipped += 1;
        return;
      }

      // A track without an album is a single: it is imported with a null
      // AlbumId, and its artwork falls back to the artist avatar set above.
      let albumId = null;
      if (track.album_name) {
        const coverUrl = track.album_image || track.image;
        const cover = coverUrl
          ? await download(coverUrl, {
              contentType: "image/jpeg",
              filename: `${track.album_id ?? "album"}.jpg`,
            }).catch(() => null)
          : null;

        const album = await catalog.resolveAlbum(`${artist.id}::${normalizeTitle(track.album_name)}`, {
          title: track.album_name,
          artistId: artist.id,
          releaseDate: track.releasedate || null,
          cover,
        });
        albumId = album.id;
      }

      // track.audio is used rather than track.audiodownload: both return the
      // same bytes, but audiodownload answers with a text/html content type and
      // BlobService stores whatever header it is handed.
      const audio = await download(track.audio, {
        contentType: "audio/mpeg",
        filename: `${track.id ?? "track"}.mp3`,
      });

      const id = await catalog.createTrack({
        title: track.name,
        durationSeconds,
        artistId: artist.id,
        albumId,
        genres: track.musicinfo?.tags?.genres ?? [],
        audio,
      });

      // Keep the dedupe set current so a duplicate later in the same run is
      // recognised without another round trip.
      existing.add(normalizeTitle(track.name));

      console.log(`  ✓ ${label}  ${id}`);
      ok += 1;
    } catch (err) {
      console.log(`  ✗ ${label}: ${err.message}`);
      failed += 1;
    }
  });

  return { ok, skipped, failed };
}

async function main() {
  const args = parseArgs(process.argv);
  const api = (option(args, "api", "SONARA_API", "http://localhost:5094/api")).replace(/\/+$/, "");
  const dryRun = Boolean(args["dry-run"]);

  const source = typeof args.source === "string" ? args.source.toLowerCase() : args.dir ? "folder" : null;
  if (source !== "folder" && source !== "jamendo") {
    fail('Choose a source: --source jamendo, or --dir "<path>" for a local folder.');
  }
  if (source === "folder" && (!args.dir || args.dir === true)) {
    fail('Missing --dir. Example: --dir "D:\\SonaraMusic"');
  }

  let token = option(args, "token", "SONARA_ADMIN_TOKEN");
  if (!token && !dryRun) {
    const email = option(args, "email", "SONARA_ADMIN_EMAIL");
    const password = option(args, "password", "SONARA_ADMIN_PASSWORD");
    if (!email || !password) {
      fail(
        "Provide an Admin token or credentials.\n" +
          "    --token, or --email/--password, or SONARA_ADMIN_TOKEN, or SONARA_ADMIN_EMAIL + SONARA_ADMIN_PASSWORD.",
      );
    }
    token = await login(api, email, password);
    console.log("  Signed in.");
  }

  const result =
    source === "jamendo"
      ? await importJamendo(args, api, token, dryRun)
      : await importFolder(args, api, token, dryRun);

  console.log(
    `\n  Done. ${result.ok} imported, ${result.skipped} skipped, ${result.failed} failed.${dryRun ? " (dry run)" : ""}\n`,
  );

  if (result.failed > 0) process.exitCode = 1;
}

main().catch((err) => fail(err?.message ?? String(err)));
