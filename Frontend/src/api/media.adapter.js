export function pick(source, ...keys) {
  if (!source) return undefined;
  for (const key of keys) {
    if (source[key] !== undefined && source[key] !== null) return source[key];
    const pascal = key.charAt(0).toUpperCase() + key.slice(1);
    if (source[pascal] !== undefined && source[pascal] !== null) return source[pascal];
  }
  return undefined;
}

export function unwrap(payload) {
  return pick(payload, "data", "result", "value") ?? payload ?? {};
}

const KIND_ALIASES = {
  track: "track", song: "track", audio: "track",
  artist: "artist", performer: "artist",
  album: "album", release: "album",
  playlist: "playlist",
  podcast: "podcast", show: "podcast", episode: "podcast",
};

function normalizeKind(raw, fallback) {
  const value = String(raw ?? fallback ?? "").toLowerCase();
  return KIND_ALIASES[value] ?? fallback ?? "track";
}

export function toSeconds(value) {
  if (value == null) return 0;
  if (typeof value === "number") return value > 10000 ? Math.round(value / 1000) : value;
  const parts = String(value).split(":").map(Number);
  if (parts.some(Number.isNaN)) return 0;
  return parts.reduce((acc, part) => acc * 60 + part, 0);
}

export function normalizeMediaItem(raw, fallbackKind) {
  if (!raw) return null;

  const kind = normalizeKind(pick(raw, "type", "kind", "entityType"), fallbackKind);
  const id = pick(raw, "id", "trackId", "artistId", "albumId", "playlistId", "guid");
  const title = pick(raw, "title", "name", "trackName", "albumName", "displayName");

  const artist = pick(raw, "artistName", "artist", "author", "owner", "creator");
  const artistName = artist && typeof artist === "object" ? pick(artist, "name", "title") : artist;

  return {
    id: String(id ?? `${kind}-${title ?? Math.random().toString(36).slice(2)}`),
    kind,
    title: title ?? "Unknown",
    subtitle: artistName ?? pick(raw, "description", "subtitle") ?? "",
    imageUrl:
      pick(raw, "imageUrl", "coverUrl", "artworkUrl", "pictureUrl", "thumbnailUrl") ?? null,
    audioUrl: pick(raw, "audioUrl", "streamUrl", "url", "fileUrl", "blobUrl") ?? null,
    durationSeconds: toSeconds(pick(raw, "durationSeconds", "duration", "lengthSeconds")),
    raw,
  };
}

export const BUCKETS = [
  ["tracks", ["tracks", "songs"], "track"],
  ["artists", ["artists"], "artist"],
  ["albums", ["albums"], "album"],
  ["playlists", ["playlists"], "playlist"],
  ["podcasts", ["podcasts", "shows"], "podcast"],
];

export const EMPTY_RESULTS = Object.freeze({
  tracks: [], artists: [], albums: [], playlists: [], podcasts: [], total: 0,
});

export function normalizeBuckets(payload) {
  const body = unwrap(payload);
  const out = { tracks: [], artists: [], albums: [], playlists: [], podcasts: [], total: 0 };

  let matched = false;
  for (const [target, keys, kind] of BUCKETS) {
    const list = pick(body, ...keys);
    const items = Array.isArray(list) ? list : pick(list ?? {}, "items", "results");
    if (Array.isArray(items)) {
      matched = true;
      out[target] = items.map((item) => normalizeMediaItem(item, kind)).filter(Boolean);
    }
  }

  if (!matched) {
    const flat = pick(body, "items", "results", "hits");
    if (Array.isArray(flat)) {
      for (const raw of flat) {
        const item = normalizeMediaItem(raw);
        if (!item) continue;
        const bucket = `${item.kind}s`;
        if (out[bucket]) out[bucket].push(item);
        else out.tracks.push(item);
      }
    }
  }

  out.total =
    Number(pick(body, "total", "totalCount", "count")) ||
    BUCKETS.reduce((sum, [target]) => sum + out[target].length, 0);

  return out;
}
