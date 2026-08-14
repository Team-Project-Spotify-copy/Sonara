import { createContext } from "react";

/**
 * Контекст акаунта окремо від провайдера — та сама вимога Fast Refresh, що і
 * для player.store: модуль з компонентом не експортує нічого, крім компонента.
 */
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
