import { createContext, useContext } from "react";

export const AccountContext = createContext(null);

/**
 * "loading" until the stored session has been checked against the API,
 * then "authenticated" or "anonymous".
 */
export const AUTH_STATUS = Object.freeze({
  LOADING: "loading",
  AUTHENTICATED: "authenticated",
  ANONYMOUS: "anonymous",
});

export function useAccount() {
  const context = useContext(AccountContext);

  if (!context) {
    throw new Error("useAccount must be used within an AccountProvider");
  }

  return context;
}
