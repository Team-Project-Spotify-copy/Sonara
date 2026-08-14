import { createContext } from "react";

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
