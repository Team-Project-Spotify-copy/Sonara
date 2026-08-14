import { api, API_BASE, getAccessToken } from "./client";

// Обгортки рівно над тими ендпоінтами, які описані в API_CONTRACT.md.
// Нових маршрутів тут не вигадуємо.

/** GET /api/tracks -> PaginatedList<Track> */
export async function getTracks({ page = 1, pageSize = 20, sort, artistId } = {}) {
  const { data } = await api.get("/tracks", {
    params: { page, pageSize, sort, artistId },
  });
  return data;
}

/** GET /api/tracks/{id} -> TrackDetails */
export async function getTrack(id) {
  const { data } = await api.get(`/tracks/${id}`);
  return data;
}

/** GET /api/tracks/{id}/stream -> TrackStream (потрібен токен) */
export async function getTrackStream(id) {
  const { data } = await api.get(`/tracks/${id}/stream`);
  return data;
}

/** GET /api/tracks/{id}/like -> TrackLikeState */
export async function getLikeState(id) {
  const { data } = await api.get(`/tracks/${id}/like`);
  return data;
}

/** POST/DELETE /api/tracks/{id}/like -> TrackLikeState (ідемпотентні) */
export async function setLiked(id, liked) {
  const { data } = liked
    ? await api.post(`/tracks/${id}/like`)
    : await api.delete(`/tracks/${id}/like`);
  return data;
}

/**
 * POST /api/tracks/{id}/listen -> ListenRegistration.
 * Викликається один раз на відтворення, а не на кожен тік прогресу.
 */
export async function registerListen(id, durationListenedMs) {
  const { data } = await api.post(`/tracks/${id}/listen`, {
    durationListenedMs: Math.max(0, Math.round(durationListenedMs)),
  });
  return data;
}

/**
 * Той самий POST /api/tracks/{id}/listen, але придатний для виклику під час
 * вивантаження документа (pagehide / visibilitychange -> hidden).
 *
 * Звичайний axios-запит у цей момент буде скасований разом зі сторінкою, тому
 * використовуємо fetch з keepalive: браузер зобов'язаний завершити його вже після
 * того, як документ помер. sendBeacon тут - лише запасний варіант: він не вміє
 * ставити заголовок Authorization, а бекенд бере ідентичність ВИКЛЮЧНО з токена.
 *
 * @returns {boolean} чи вдалося передати запит транспорту
 */
export function sendListenBeacon(id, durationListenedMs) {
  const token = getAccessToken();
  if (!id || !token) return false;

  const url = `${API_BASE}/tracks/${id}/listen`;
  const body = JSON.stringify({
    durationListenedMs: Math.max(0, Math.round(durationListenedMs)),
  });

  if (typeof fetch === "function") {
    try {
      // Відповідь навмисно не читаємо - сторінки, яка могла б її обробити, вже немає.
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
      /* падаємо у sendBeacon нижче */
    }
  }

  if (typeof navigator !== "undefined" && typeof navigator.sendBeacon === "function") {
    return navigator.sendBeacon(url, new Blob([body], { type: "application/json" }));
  }

  return false;
}

/** GET /api/artists/{id} -> Artist */
export async function getArtist(id) {
  const { data } = await api.get(`/artists/${id}`);
  return data;
}
