import api from "./client.js";
import { ENDPOINTS, USE_MOCKS } from "./endpoints.js";
import { normalizeMediaItem, unwrap, pick } from "./media.adapter.js";

const MOCK_LIBRARY = [
  { id: "lib-1", type: "playlist", title: "Liked Songs", artistName: "247 songs" },
  { id: "lib-2", type: "album", title: "Midnight Drive", artistName: "Kavinsky" },
  { id: "lib-3", type: "artist", title: "Aurora", artistName: "Artist" },
  { id: "lib-4", type: "album", title: "Blue Hour", artistName: "Nilüfer Yanya" },
  { id: "lib-5", type: "artist", title: "Bonobo", artistName: "Artist" },
  { id: "lib-6", type: "artist", title: "Sofia Kourtesis", artistName: "Artist" },
  { id: "lib-7", type: "playlist", title: "Late Night Focus", artistName: "58 songs" },
  { id: "lib-8", type: "album", title: "In Colour", artistName: "Jamie xx" },
];

/**
 * @param {{signal?: AbortSignal}} [args]
 * @returns {Promise<object[]>}
 */
export async function libraryQuery({ signal } = {}) {
  if (USE_MOCKS) {
    return MOCK_LIBRARY.map((item) => normalizeMediaItem(item, item.type));
  }

  const response = await api.get(ENDPOINTS.library, { signal });
  const body = unwrap(response.data);
  const items = Array.isArray(body) ? body : pick(body, "items", "results", "library") ?? [];

  return items.map((item) => normalizeMediaItem(item)).filter(Boolean);
}
