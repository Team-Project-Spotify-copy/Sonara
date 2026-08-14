import axios from "axios";

// Той самий конвеншн, що і в Login/Register: базовий URL береться з VITE_API
// і вже містить /api (напр. http://localhost:5094/api).
export const API_BASE =
  import.meta.env.VITE_API || "http://localhost:5094/api";

export const api = axios.create({
  baseURL: API_BASE,
  // refresh-токен лежить у HttpOnly cookie, тому кукі треба надсилати.
  withCredentials: true,
});

export function getAccessToken() {
  return localStorage.getItem("accessToken");
}

function setAccessToken(token) {
  if (token) {
    localStorage.setItem("accessToken", token);
  } else {
    localStorage.removeItem("accessToken");
  }
}

api.interceptors.request.use((config) => {
  const token = getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// Один спільний refresh на всі паралельні запити, щоб плеєр не влаштував
// шквал /auth/refresh, коли токен протух під час відтворення.
let refreshPromise = null;

function refreshAccessToken() {
  refreshPromise ??= axios
    .post(`${API_BASE}/auth/refresh`, null, { withCredentials: true })
    .then((response) => {
      const token = response.data?.accessToken ?? null;
      setAccessToken(token);
      return token;
    })
    .catch(() => {
      setAccessToken(null);
      return null;
    })
    .finally(() => {
      refreshPromise = null;
    });

  return refreshPromise;
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // Оновлюємо токен рівно один раз на запит: інакше цикл 401 -> refresh -> 401.
    if (error.response?.status !== 401 || !original || original._retried) {
      return Promise.reject(error);
    }

    original._retried = true;

    const token = await refreshAccessToken();
    if (!token) {
      return Promise.reject(error);
    }

    original.headers = { ...original.headers, Authorization: `Bearer ${token}` };
    return api(original);
  },
);

/** Дістає стабільний код помилки з конверта бекенда (див. API_CONTRACT.md §2). */
export function errorCode(error) {
  return error?.response?.data?.code ?? "network_error";
}
