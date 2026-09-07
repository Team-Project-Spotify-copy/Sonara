import axios from "axios";

export const API_BASE =
  import.meta.env.VITE_API || "http://localhost:5094/api";

export const api = axios.create({
  baseURL: API_BASE,
  withCredentials: true,
});

export function getAccessToken() {
  return localStorage.getItem("accessToken");
}

export const AUTH_EXPIRED_EVENT = "auth:expired";

export function setAccessToken(token) {
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
      // Tell the app the session is gone so it can clear state and redirect.
      // Without a listener the user would sit on a page that keeps 401ing.
      window.dispatchEvent(new CustomEvent(AUTH_EXPIRED_EVENT));
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

export function errorCode(error) {
  return error?.response?.data?.code ?? "network_error";
}

export default api;
