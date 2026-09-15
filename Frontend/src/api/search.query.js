import api from "@api/client.js";
import { ENDPOINTS } from "@api/endpoints.js";
import { normalizeBuckets, EMPTY_RESULTS } from "@api/media.adapter.js";

export { EMPTY_RESULTS };

export async function searchQuery({ q, types, page = 1, pageSize = 20, signal }) {
  const params = { q, query: q, page, pageSize };
  if (types?.length) params.types = types.join(",");

  const response = await api.get(ENDPOINTS.search, { params, signal });
  return normalizeBuckets(response.data);
}
