import { createContext, useContext } from "react";

/**
 * Контекст плеєра та все, що не є компонентом, живе окремо від провайдера.
 * Це вимога Fast Refresh: модуль, який експортує компонент, не повинен
 * експортувати нічого іншого — інакше правка провайдера перезавантажує
 * сторінку цілком і відтворення обривається.
 */
export const PlayerContext = createContext(null);

export const REPEAT_MODES = ["off", "all", "one"];

/** Три стани UI з макета: 324:942, 324:927, 324:995. */
export const VIEW_MODES = ["normal", "fullscreen", "lyrics"];

export function usePlayer() {
  const context = useContext(PlayerContext);

  if (!context) {
    throw new Error("usePlayer must be used within a PlayerProvider");
  }

  return context;
}
