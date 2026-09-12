import { createContext, useContext } from "react";

export const PlayerContext = createContext(null);

export const REPEAT_MODES = ["off", "all", "one"];

export const VIEW_MODES = ["normal", "fullscreen"];

export function usePlayer() {
  const context = useContext(PlayerContext);

  if (!context) {
    throw new Error("usePlayer must be used within a PlayerProvider");
  }

  return context;
}
