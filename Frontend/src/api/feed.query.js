import api from "@api/client.js";
import { getAccessToken } from "@api/client.js";
import { USE_MOCKS } from "@api/endpoints.js";
import { normalizeMediaItem, unwrap, pick } from "@api/media.adapter.js";

export const SHELF_DEFINITIONS = [
  { key: "weekly", title: "Weekly selections", shape: "square" },
  { key: "foryou", title: "For you", shape: "square" },
  { key: "artists", title: "Recommended artists", shape: "round" },
  { key: "albums", title: "Recommended albums", shape: "square" },
  { key: "podcasts", title: "Recommended podcasts", shape: "square" },
  { key: "recent", title: "Recent", shape: "square" },
];

const SHELF_SIZE = 20;

function mockShelf(key, kind, count = 7) {
  return Array.from({ length: count }, (_, i) =>
    normalizeMediaItem(
      {
        id: `${key}-${i}`,
        type: kind,
        title: `Track ${i + 1}`,
        artistName: "Sonara Artist",
        durationSeconds: 150 + i * 17,
        audioUrl: null,
      },
      kind,
    ),
  );
}

function mockFeed() {
  return SHELF_DEFINITIONS.map((shelf) => ({
    ...shelf,
    items: mockShelf(shelf.key, shelf.key === "artists" ? "artist" : "track"),
  }));
}

/**
 * There is no /api/feed on this branch. Rather than stand up a parallel music
 * stack, the shelves are assembled from the existing catalog endpoints, which
 * already return TrackDto via MusicCatalogService:
 *
 *   GET /api/tracks?sort=Popular|Newest|Title&page&pageSize   (anonymous)
 *   GET /api/podcasts                                          (authenticated)
 *   GET /api/history                                           (authenticated)
 *
 * /api/tracks/popular and /api/albums/popular are deliberately avoided: for
 * anonymous callers CachedMusicCatalogService routes them through Redis, which
 * is optional in this environment, and RedisCacheService has no error handling.
 * GetTracksAsync is never cached, so it works with or without Redis.
 */
async function fetchTracks({ sort, pageSize = SHELF_SIZE, signal }) {
  const response = await api.get("/tracks", {
    params: { page: 1, pageSize, sort },
    signal,
  });
  const body = unwrap(response.data);
  const items = pick(body, "items", "results") ?? [];
  return Array.isArray(items) ? items : [];
}

/** Artist/album shelves are derived from the tracks already fetched, so no
 *  extra endpoint is needed (the catalog exposes no artist/album listing). */
function uniqueArtists(tracks) {
  const seen = new Map();
  for (const t of tracks) {
    const id = pick(t, "artistId");
    if (!id || seen.has(id)) continue;
    seen.set(id, {
      id,
      type: "artist",
      title: pick(t, "artistName") ?? "Unknown artist",
      subtitle: "Artist",
      imageUrl: pick(t, "artworkUrl") ?? null,
    });
  }
  return [...seen.values()];
}

function uniqueAlbums(tracks) {
  const seen = new Map();
  for (const t of tracks) {
    const id = pick(t, "albumId");
    if (!id || seen.has(id)) continue;
    seen.set(id, {
      id,
      type: "album",
      title: pick(t, "albumTitle") ?? "Unknown album",
      subtitle: pick(t, "artistName") ?? "Album",
      imageUrl: pick(t, "artworkUrl") ?? null,
    });
  }
  return [...seen.values()];
}

async function safe(promise, fallback = []) {
  try {
    return await promise;
  } catch {
    // A shelf that cannot load must not take the whole page down.
    return fallback;
  }
}

export async function feedQuery({ signal } = {}) {
  if (USE_MOCKS) {
    return mockFeed();
  }

  const authenticated = Boolean(getAccessToken());

  const [popular, newest, byTitle] = await Promise.all([
    safe(fetchTracks({ sort: "Popular", signal })),
    safe(fetchTracks({ sort: "Newest", signal })),
    safe(fetchTracks({ sort: "Title", signal })),
  ]);

  // Podcasts and history exist only for a signed-in user; anonymous visitors
  // get those shelves empty rather than a fabricated stand-in.
  const podcasts = authenticated
    ? await safe(api.get("/podcasts", { signal }).then((r) => unwrap(r.data) ?? []))
    : [];

  const history = authenticated
    ? await safe(
        api.get("/history", { params: { page: 1, pageSize: SHELF_SIZE }, signal }).then((r) => {
          const body = unwrap(r.data);
          const items = pick(body, "items", "results") ?? [];
          return Array.isArray(items) ? items.map((h) => pick(h, "track")).filter(Boolean) : [];
        }),
      )
    : [];

  const catalog = popular.length ? popular : newest;

  const bucket = {
    weekly: popular.map((t) => normalizeMediaItem(t, "track")),
    foryou: newest.map((t) => normalizeMediaItem(t, "track")),
    artists: uniqueArtists(catalog).map((a) => normalizeMediaItem(a, "artist")),
    albums: uniqueAlbums(catalog).map((a) => normalizeMediaItem(a, "album")),
    podcasts: (Array.isArray(podcasts) ? podcasts : []).map((p) =>
      normalizeMediaItem(
        {
          id: pick(p, "id"),
          type: "podcast",
          title: pick(p, "title"),
          subtitle: pick(p, "authorName") ?? "Podcast",
          imageUrl: pick(p, "coverUrl") ?? null,
        },
        "podcast",
      ),
    ),
    recent: history.length
      ? history.map((t) => normalizeMediaItem(t, "track"))
      : byTitle.map((t) => normalizeMediaItem(t, "track")),
  };

  return SHELF_DEFINITIONS.map((shelf) => ({
    ...shelf,
    items: (bucket[shelf.key] ?? []).filter(Boolean),
  }));
}
