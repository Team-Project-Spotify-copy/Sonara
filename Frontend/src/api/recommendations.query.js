import api from "./client.js";
import { ENDPOINTS } from "./endpoints.js";
import { normalizeMediaItem, unwrap, pick } from "./media.adapter.js";

const DEFAULT_COUNT = 12;

/** Items arrive as { track, score, matchedGenres }; the match drives the subtitle. */
export async function recommendationsQuery({ count = DEFAULT_COUNT, signal } = {}) {
  const response = await api.get(ENDPOINTS.recommendations, {
    params: { count },
    signal,
  });

  const body = unwrap(response.data);
  const items = pick(body, "items", "results") ?? [];

  return {
    strategy: pick(body, "strategy") ?? "Popular",
    topGenres: pick(body, "topGenres") ?? [],
    items: (Array.isArray(items) ? items : [])
      .map((entry) => {
        const item = normalizeMediaItem(pick(entry, "track") ?? entry, "track");
        if (!item) return null;

        const matched = pick(entry, "matchedGenres") ?? [];
        return {
          ...item,
          subtitle: matched.length ? `${item.artistName ?? item.subtitle} • ${matched[0]}` : item.subtitle,
        };
      })
      .filter(Boolean),
  };
}
