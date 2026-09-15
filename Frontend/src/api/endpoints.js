export const ENDPOINTS = {
  search: "/search",
  feed: "/feed",
  library: "/library",
  recommendations: "/recommendations",
  stream: (trackId) => `/tracks/${trackId}/stream`,
};

export const USE_MOCKS = import.meta.env.VITE_USE_MOCKS !== "false";
