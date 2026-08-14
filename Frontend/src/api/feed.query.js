import api from "./client.js";
import { ENDPOINTS, USE_MOCKS } from "./endpoints.js";
import { normalizeMediaItem, unwrap, pick } from "./media.adapter.js";

export const SHELF_DEFINITIONS = [
  { key: "weekly", title: "Weekly selections", shape: "square" },
  { key: "foryou", title: "For you", shape: "square" },
  { key: "artists", title: "Recommended artists", shape: "round" },
  { key: "albums", title: "Recommended albums", shape: "square" },
  { key: "podcasts", title: "Recommended podcasts", shape: "square" },
  { key: "recent", title: "Recent", shape: "square" },
];

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
 * @param {{signal?: AbortSignal}} [args]
 * @returns {Promise<Array<{key: string, title: string, shape: string, items: object[]}>>}
 */
export async function feedQuery({ signal } = {}) {
  if (USE_MOCKS) {
    return mockFeed();
  }

  const response = await api.get(ENDPOINTS.feed, { signal });
  const body = unwrap(response.data);

  return SHELF_DEFINITIONS.map((shelf) => {
    const raw = pick(body, shelf.key) ?? [];
    const items = Array.isArray(raw) ? raw : pick(raw, "items", "results") ?? [];
    return {
      ...shelf,
      items: items
        .map((item) => normalizeMediaItem(item, shelf.shape === "round" ? "artist" : "track"))
        .filter(Boolean),
    };
  });
}
