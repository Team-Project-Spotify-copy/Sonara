import React, { createContext, useState, useEffect } from "react";

export const AccountContext = createContext({
  userId: null,
  setUserId: () => {},
  email: null,
  setEmail: () => {},
  accessToken: null,
  setAccessToken: () => {},
  clear: () => {},
  isAuth: () => null,
  getEmail: () => null,
});

export const AccountProvider = ({ children }) => {
  const [userId, setUserId] = useState(() => {
    return localStorage.getItem("userId") || null;
  });

  const [email, setEmail] = useState(() => {
    return localStorage.getItem("email") || null;
  });

  const [accessToken, setAccessToken] = useState(() => {
    return localStorage.getItem("accessToken") || null;
  });

  useEffect(() => {
    if (userId) {
      localStorage.setItem("userId", userId);
    } else {
      localStorage.removeItem("userId");
    }

    if (email) {
      localStorage.setItem("email", email);
    } else {
      localStorage.removeItem("email");
    }

    if (accessToken) {
      localStorage.setItem("accessToken", accessToken);
    } else {
      localStorage.removeItem("accessToken");
    }

  }, [userId, email, accessToken]);

  const clear = () => {
    setUserId(null);
    setEmail(null);
    setAccessToken(null);
  };

  const isAuth = () => email !== null;

  return (
    <AccountContext.Provider
      value={{ email, setEmail, accessToken, setAccessToken, userId, setUserId, clear, isAuth }}
    >
      {children}
    </AccountContext.Provider>
  );
};
