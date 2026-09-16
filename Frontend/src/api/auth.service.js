import api, { API_BASE, setAccessToken } from "@api/client.js";
import axios from "axios";

/**
 * Every auth call goes through the shared `api` client so it inherits the
 * bearer-token request interceptor, `withCredentials` (needed for the HttpOnly
 * refresh cookie) and the single-flight refresh on 401.
 */

function messageFor(error, fallback) {
  const data = error?.response?.data;

  if (typeof data === "string" && data.trim()) return data;

  // The API returns { statusCode, code, message, errors, traceId }.
  const fieldErrors = data?.errors;
  if (fieldErrors && typeof fieldErrors === "object") {
    const first = Object.values(fieldErrors).flat().filter(Boolean)[0];
    if (first) return first;
  }

  if (data?.message) return data.message;
  if (!error?.response) return "Cannot reach the server. Check your connection and try again.";

  return fallback;
}

export class AuthError extends Error {
  constructor(error, fallback) {
    super(messageFor(error, fallback));
    this.name = "AuthError";
    this.status = error?.response?.status ?? 0;
    this.code = error?.response?.data?.code ?? "network_error";
    this.fieldErrors = error?.response?.data?.errors ?? null;
  }
}

export async function register({ email, username, password, token }) {
  try {
    const { data } = await api.post("/auth/register", { email, username, password, token });
    return { userId: data?.userId ?? null, accessToken: data?.accessToken ?? null };
  } catch (error) {
    throw new AuthError(error, "Could not create your account. Please try again.");
  }
}

export async function login({ email, password, token }) {
  try {
    const { data } = await api.post("/auth/login", { email, password, token });
    return { userId: data?.userId ?? null, accessToken: data?.accessToken ?? null };
  } catch (error) {
    throw new AuthError(error, "Could not sign you in. Please try again.");
  }
}

export async function logout() {
  // A bare instance: the shared client would try to refresh on the 401 we may
  // get here, which is pointless while tearing the session down.
  try {
    await axios.post(`${API_BASE}/auth/logout`, null, { withCredentials: true });
  } catch {
    // Logging out must always succeed locally, even if the server call fails.
  } finally {
    setAccessToken(null);
  }
}

export async function getCurrentUser() {
  const { data } = await api.get("/profile");
  return data;
}

export async function forgotPassword(email) {
  try {
    const { data } = await api.post("/auth/forgot-password", { email });
    return data;
  } catch (error) {
    throw new AuthError(error, "Could not send the reset code. Please try again.");
  }
}

export async function verifyResetCode({ email, code }) {
  try {
    const { data } = await api.post("/auth/verify-reset-code", { email, code });
    return data?.resetToken ?? null;
  } catch (error) {
    throw new AuthError(error, "That code is not valid. Please try again.");
  }
}

export async function resetPassword({ email, resetToken, newPassword, confirmPassword }) {
  try {
    const { data } = await api.post("/auth/reset-password", {
      email,
      resetToken,
      newPassword,
      confirmPassword,
    });
    return data;
  } catch (error) {
    throw new AuthError(error, "Could not reset your password. Please try again.");
  }
}
