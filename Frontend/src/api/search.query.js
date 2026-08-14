import api from "./client.js";
import { ENDPOINTS } from "./endpoints.js";
import { normalizeBuckets, EMPTY_RESULTS } from "./media.adapter.js";

export { EMPTY_RESULTS };

/**
 * @param {{q: string, types?: string[], page?: number, pageSize?: number, signal?: AbortSignal}} args
 * @returns {Promise<typeof EMPTY_RESULTS>}
 */
export async function searchQuery({ q, types, page = 1, pageSize = 20, signal }) {
  const params = { q, query: q, page, pageSize };
  if (types?.length) params.types = types.join(",");

  const response = await api.get(ENDPOINTS.search, { params, signal });
  return normalizeBuckets(response.data);
}
