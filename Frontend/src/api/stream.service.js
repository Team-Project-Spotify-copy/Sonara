import { ENDPOINTS } from "./endpoints.js";

export function resolveStreamUrl(track) {
  if (!track) return null;
  if (track.audioUrl) return track.audioUrl;
  if (!track.id) return null;

  const base = import.meta.env.VITE_API ?? "";
  return `${base}${ENDPOINTS.stream(track.id)}`;
}

export function isPlayable(track) {
  return Boolean(resolveStreamUrl(track));
}
