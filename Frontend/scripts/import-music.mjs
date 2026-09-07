#!/usr/bin/env node
/**
 * Development-only bulk importer for local audio files.
 *
 * It adds nothing to the architecture: every file is POSTed to the existing
 * admin endpoint, so uploads go through the existing BlobService into the
 * existing Azure container ("audio/tracks") and the Track row is created by
 * the existing AdminMusicService. No audio ever touches the repository.
 *
 *   POST /api/auth/login          -> obtain a JWT (needs an Admin account)
 *   GET  /api/tracks?pageSize=100 -> discover existing artistId/artistName pairs
 *   POST /api/admin/tracks        -> multipart upload + Track creation
 *
 * Usage:
 *   node scripts/import-music.mjs --dir "D:\\SonaraMusic" --email you@example.com --password ****
 *   node scripts/import-music.mjs --dir "D:\\SonaraMusic" --token "<jwt>"
 *
 * Options:
 *   --dir <path>        folder to scan (required)
 *   --api <url>         API base, default http://localhost:5094/api
 *   --token <jwt>       use an existing Admin token instead of logging in
 *   --email/--password  Admin credentials (Development reCAPTCHA bypass is used)
 *   --artist <name>     reuse the existing artist with this name (case-insensitive)
 *   --artist-id <guid>  reuse this artist id directly
 *   --album-id <guid>   attach every imported track to this album
 *   --dry-run           show what would happen, upload nothing
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

async function main() {
  const args = parseArgs(process.argv);
  const api = (args.api ?? "http://localhost:5094/api").replace(/\/+$/, "");
  const dir = args.dir;
  const dryRun = Boolean(args["dry-run"]);

  if (!dir || dir === true) fail('Missing --dir. Example: --dir "D:\\SonaraMusic"');

  const files = await listAudioFiles(dir);
  if (files.length === 0) fail(`No audio files found in ${dir}`);
  console.log(`\n  Found ${files.length} audio file(s) in ${dir}`);

  let token = typeof args.token === "string" ? args.token : null;
  if (!token && !dryRun) {
    if (typeof args.email !== "string" || typeof args.password !== "string") {
      fail("Provide --token, or --email and --password for an Admin account.");
    }
    token = await login(api, args.email, args.password);
    console.log("  Signed in.");
  }

  const artists = dryRun && !token ? new Map() : await loadArtists(api, token);
  console.log(`  Known artists in catalog: ${artists.size}`);

  let artistId = typeof args["artist-id"] === "string" ? args["artist-id"] : null;
  if (!artistId && typeof args.artist === "string") {
    const found = artists.get(args.artist.toLowerCase());
    if (!found) {
      fail(
        `No existing artist named "${args.artist}". Known: ${[...artists.values()].map((a) => a.name).join(", ") || "(none)"}\n` +
          "    The API has no artist-creation endpoint; pass --artist-id or add the Artist row directly.",
      );
    }
    artistId = found.id;
  }
  if (!artistId && artists.size > 0) {
    artistId = [...artists.values()][0].id;
    console.log(`  No --artist given, defaulting to existing artist "${[...artists.values()][0].name}".`);
  }
  if (!artistId && !dryRun) {
    fail("No artist available. Pass --artist-id (the API cannot create artists).");
  }

  const albumId = typeof args["album-id"] === "string" ? args["album-id"] : null;

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

    const form = new FormData();
    form.append("Title", title);
    form.append("DurationSeconds", String(durationSeconds));
    form.append("ArtistId", artistId);
    if (albumId) form.append("AlbumId", albumId);
    form.append("AudioFile", new Blob([await readFile(file)], { type: MIME[ext] ?? "application/octet-stream" }), name);

    const res = await fetch(`${api}/admin/tracks`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: form,
    });

    if (res.ok) {
      const id = await res.json();
      console.log(`  ✓ ${name} -> "${title}"  ${id}`);
      ok += 1;
    } else {
      console.log(`  ✗ ${name}: HTTP ${res.status} ${(await res.text()).slice(0, 160)}`);
      failed += 1;
    }
  }

  console.log(`\n  Done. ${ok} imported, ${failed} failed.${dryRun ? " (dry run)" : ""}\n`);
}

main().catch((err) => fail(err?.message ?? String(err)));
