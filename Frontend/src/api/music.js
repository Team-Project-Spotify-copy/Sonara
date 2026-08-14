import { api, API_BASE, getAccessToken } from "./client";

export async function getTracks({ page = 1, pageSize = 20, sort, artistId } = {}) {
  const { data } = await api.get("/tracks", {
    params: { page, pageSize, sort, artistId },
  });
  return data;
}

export async function getTrack(id) {
  const { data } = await api.get(`/tracks/${id}`);
  return data;
}

export async function getTrackStream(id) {
  const { data } = await api.get(`/tracks/${id}/stream`);
  return data;
}

export async function getLikeState(id) {
  const { data } = await api.get(`/tracks/${id}/like`);
  return data;
}

export async function setLiked(id, liked) {
  const { data } = liked
    ? await api.post(`/tracks/${id}/like`)
    : await api.delete(`/tracks/${id}/like`);
  return data;
}

export async function registerListen(id, durationListenedMs) {
  const { data } = await api.post(`/tracks/${id}/listen`, {
    durationListenedMs: Math.max(0, Math.round(durationListenedMs)),
  });
  return data;
}

export function sendListenBeacon(id, durationListenedMs) {
  const token = getAccessToken();
  if (!id || !token) return false;

  const url = `${API_BASE}/tracks/${id}/listen`;
  const body = JSON.stringify({
    durationListenedMs: Math.max(0, Math.round(durationListenedMs)),
  });

  if (typeof fetch === "function") {
    try {
      fetch(url, {
        method: "POST",
        keepalive: true,
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body,
      }).catch(() => {});
      return true;
    } catch {
    }
  }

  if (typeof navigator !== "undefined" && typeof navigator.sendBeacon === "function") {
    return navigator.sendBeacon(url, new Blob([body], { type: "application/json" }));
  }

  return false;
}

export async function getArtist(id) {
  const { data } = await api.get(`/artists/${id}`);
  return data;
}
