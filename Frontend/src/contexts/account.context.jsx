import React, { useCallback, useEffect, useMemo, useState } from "react";
import { AccountContext, AUTH_STATUS } from "./account.store";
import { AUTH_EXPIRED_EVENT, getAccessToken, setAccessToken } from "../api/client.js";
import * as authService from "../api/auth.service.js";

export const AccountProvider = ({ children }) => {
  // The access token in localStorage is the single source of truth for "is there
  // a session?". Email/username are display data hydrated from /api/profile.
  const [status, setStatus] = useState(() =>
    getAccessToken() ? AUTH_STATUS.LOADING : AUTH_STATUS.ANONYMOUS,
  );
  const [user, setUser] = useState(null);
  // Mirrored in state so consumers re-render when the session changes.
  const [accessToken, setAccessTokenState] = useState(() => getAccessToken());
  const [userId, setUserIdState] = useState(() => localStorage.getItem("userId"));

  const clearSession = useCallback(() => {
    setAccessToken(null);
    localStorage.removeItem("userId");
    localStorage.removeItem("email");
    setAccessTokenState(null);
    setUserIdState(null);
    setUser(null);
    setStatus(AUTH_STATUS.ANONYMOUS);
  }, []);

  const loadCurrentUser = useCallback(async () => {
    try {
      const profile = await authService.getCurrentUser();
      setUser(profile);
      setStatus(AUTH_STATUS.AUTHENTICATED);
      if (profile?.email) localStorage.setItem("email", profile.email);
      return profile;
    } catch (error) {
      // A 401 here means the stored token is dead and the refresh cookie could
      // not save it. Anything else (network, 5xx) must not sign the user out.
      if (error?.response?.status === 401) {
        clearSession();
        return null;
      }
      setStatus(AUTH_STATUS.AUTHENTICATED);
      return null;
    }
  }, [clearSession]);

  // Restore the session on mount / full page refresh.
  useEffect(() => {
    if (!getAccessToken()) {
      setStatus(AUTH_STATUS.ANONYMOUS);
      return;
    }
    void loadCurrentUser();
  }, [loadCurrentUser]);

  // The api client dispatches this when a refresh attempt fails.
  useEffect(() => {
    const onExpired = () => clearSession();
    window.addEventListener(AUTH_EXPIRED_EVENT, onExpired);
    return () => window.removeEventListener(AUTH_EXPIRED_EVENT, onExpired);
  }, [clearSession]);

  const applyCredentials = useCallback(
    async (credentials) => {
      setAccessToken(credentials.accessToken);
      setAccessTokenState(credentials.accessToken);

      if (credentials.userId) {
        localStorage.setItem("userId", credentials.userId);
        setUserIdState(credentials.userId);
      }

      setStatus(AUTH_STATUS.LOADING);
      await loadCurrentUser();
    },
    [loadCurrentUser],
  );

  const login = useCallback(
    async (credentials) => applyCredentials(await authService.login(credentials)),
    [applyCredentials],
  );

  const register = useCallback(
    async (details) => applyCredentials(await authService.register(details)),
    [applyCredentials],
  );

  const logout = useCallback(async () => {
    await authService.logout();
    clearSession();
  }, [clearSession]);

  const value = useMemo(
    () => ({
      status,
      isLoading: status === AUTH_STATUS.LOADING,
      isAuthenticated: status === AUTH_STATUS.AUTHENTICATED,
      user,
      userId,
      accessToken,
      email: user?.email ?? null,
      username: user?.username ?? null,
      login,
      register,
      logout,
      reloadUser: loadCurrentUser,
    }),
    [status, user, userId, accessToken, login, register, logout, loadCurrentUser],
  );

  return <AccountContext.Provider value={value}>{children}</AccountContext.Provider>;
};
