import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useReducer,
  useRef,
} from "react";
import { resolveStreamUrl } from "../api/stream.service.js";

const PlayerContext = createContext(null);

const REPEAT_MODES = ["off", "all", "one"];

function readVolume() {
  const stored = Number(localStorage.getItem("player.volume"));
  return Number.isFinite(stored) && stored >= 0 && stored <= 1 ? stored : 0.8;
}

const initialState = {
  queue: [],
  order: [],
  cursor: -1,
  isPlaying: false,
  position: 0,
  duration: 0,
  volume: readVolume(),
  muted: false,
  shuffle: false,
  repeat: "off",
  status: "idle",
  error: null,
};

function buildOrder(length, shuffle, startIndex = 0) {
  const indices = Array.from({ length }, (_, i) => i);
  if (!shuffle) return indices;

  for (let i = indices.length - 1; i > 0; i -= 1) {
    const j = Math.floor(Math.random() * (i + 1));
    [indices[i], indices[j]] = [indices[j], indices[i]];
  }

  const at = indices.indexOf(startIndex);
  if (at > 0) [indices[0], indices[at]] = [indices[at], indices[0]];
  return indices;
}

function reducer(state, action) {
  switch (action.type) {
    case "LOAD_QUEUE": {
      const { queue, startIndex } = action;
      const order = buildOrder(queue.length, state.shuffle, startIndex);
      return {
        ...state,
        queue,
        order,
        cursor: queue.length ? order.indexOf(startIndex) : -1,
        position: 0,
        duration: 0,
        isPlaying: queue.length > 0,
        status: queue.length ? "loading" : "idle",
        error: null,
      };
    }
    case "SET_CURSOR":
      return { ...state, cursor: action.cursor, position: 0, duration: 0, status: "loading" };
    case "PLAY":
      return { ...state, isPlaying: true };
    case "PAUSE":
      return { ...state, isPlaying: false };
    case "STOP":
      return { ...state, isPlaying: false, position: 0 };
    case "CLEAR":
      return {
        ...initialState,
        volume: state.volume,
        muted: state.muted,
        shuffle: state.shuffle,
        repeat: state.repeat,
      };
    case "SET_POSITION":
      return { ...state, position: action.position };
    case "SET_DURATION":
      return { ...state, duration: action.duration, status: "ready" };
    case "SET_VOLUME":
      return { ...state, volume: action.volume, muted: action.volume === 0 };
    case "SET_MUTED":
      return { ...state, muted: action.muted };
    case "TOGGLE_SHUFFLE": {
      const shuffle = !state.shuffle;
      const current = state.order[state.cursor] ?? 0;
      const order = buildOrder(state.queue.length, shuffle, current);
      return { ...state, shuffle, order, cursor: order.indexOf(current) };
    }
    case "CYCLE_REPEAT": {
      const nextMode = REPEAT_MODES[(REPEAT_MODES.indexOf(state.repeat) + 1) % REPEAT_MODES.length];
      return { ...state, repeat: nextMode };
    }
    case "ERROR":
      return { ...state, status: "error", error: action.error, isPlaying: false };
    default:
      return state;
  }
}

export function PlayerProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, initialState);

  const audioRef = useRef(null);
  if (audioRef.current === null && typeof Audio !== "undefined") {
    audioRef.current = new Audio();
  }

  const currentTrack =
    state.cursor >= 0 ? state.queue[state.order[state.cursor]] ?? null : null;

  const streamUrl = resolveStreamUrl(currentTrack);
  const hasActiveTrack = currentTrack !== null;

  const next = useCallback(() => {
    if (state.cursor < 0) return;
    const last = state.order.length - 1;
    if (state.cursor < last) {
      dispatch({ type: "SET_CURSOR", cursor: state.cursor + 1 });
    } else if (state.repeat === "all") {
      dispatch({ type: "SET_CURSOR", cursor: 0 });
    } else {
      dispatch({ type: "STOP" });
    }
  }, [state.cursor, state.order.length, state.repeat]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return undefined;

    const onTime = () => dispatch({ type: "SET_POSITION", position: audio.currentTime });
    const onMeta = () => dispatch({ type: "SET_DURATION", duration: audio.duration || 0 });
    const onError = () =>
      dispatch({ type: "ERROR", error: new Error("Playback failed for this track") });
    const onEnded = () => {
      if (state.repeat === "one") {
        audio.currentTime = 0;
        audio.play().catch(() => {});
        return;
      }
      next();
    };

    audio.addEventListener("timeupdate", onTime);
    audio.addEventListener("loadedmetadata", onMeta);
    audio.addEventListener("ended", onEnded);
    audio.addEventListener("error", onError);

    return () => {
      audio.removeEventListener("timeupdate", onTime);
      audio.removeEventListener("loadedmetadata", onMeta);
      audio.removeEventListener("ended", onEnded);
      audio.removeEventListener("error", onError);
    };
  });

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;

    if (!streamUrl) {
      audio.removeAttribute("src");
      return;
    }

    if (audio.src !== streamUrl) {
      audio.src = streamUrl;
      audio.load();
    }
  }, [streamUrl]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio || !streamUrl) return;

    if (state.isPlaying) {
      audio.play().catch((error) => dispatch({ type: "ERROR", error }));
    } else {
      audio.pause();
    }
  }, [state.isPlaying, streamUrl]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;
    audio.volume = state.muted ? 0 : state.volume;
    localStorage.setItem("player.volume", String(state.volume));
  }, [state.volume, state.muted]);

  useEffect(() => {
    const audio = audioRef.current;
    return () => {
      if (!audio) return;
      audio.pause();
      audio.removeAttribute("src");
      audio.load();
    };
  }, []);

  const play = useCallback((tracks, startIndex = 0) => {
    const queue = Array.isArray(tracks) ? tracks : [tracks];
    dispatch({ type: "LOAD_QUEUE", queue, startIndex });
  }, []);

  const toggle = useCallback(() => {
    dispatch({ type: state.isPlaying ? "PAUSE" : "PLAY" });
  }, [state.isPlaying]);

  const stop = useCallback(() => {
    const audio = audioRef.current;
    if (audio) {
      audio.pause();
      audio.removeAttribute("src");
      audio.load();
    }
    dispatch({ type: "CLEAR" });
  }, []);

  const previous = useCallback(() => {
    const audio = audioRef.current;
    if (audio && audio.currentTime > 3) {
      audio.currentTime = 0;
      return;
    }
    if (state.cursor > 0) {
      dispatch({ type: "SET_CURSOR", cursor: state.cursor - 1 });
    } else if (audio) {
      audio.currentTime = 0;
    }
  }, [state.cursor]);

  const seek = useCallback((seconds) => {
    const audio = audioRef.current;
    if (!audio) return;
    audio.currentTime = seconds;
    dispatch({ type: "SET_POSITION", position: seconds });
  }, []);

  const setVolume = useCallback((volume) => dispatch({ type: "SET_VOLUME", volume }), []);
  const toggleMute = useCallback(
    () => dispatch({ type: "SET_MUTED", muted: !state.muted }),
    [state.muted],
  );
  const toggleShuffle = useCallback(() => dispatch({ type: "TOGGLE_SHUFFLE" }), []);
  const cycleRepeat = useCallback(() => dispatch({ type: "CYCLE_REPEAT" }), []);

  const value = useMemo(
    () => ({
      ...state,
      currentTrack,
      hasActiveTrack,
      streamUrl,
      play,
      toggle,
      stop,
      next,
      previous,
      seek,
      setVolume,
      toggleMute,
      toggleShuffle,
      cycleRepeat,
    }),
    [
      state,
      currentTrack,
      hasActiveTrack,
      streamUrl,
      play,
      toggle,
      stop,
      next,
      previous,
      seek,
      setVolume,
      toggleMute,
      toggleShuffle,
      cycleRepeat,
    ],
  );

  return <PlayerContext.Provider value={value}>{children}</PlayerContext.Provider>;
}

export function usePlayer() {
  const ctx = useContext(PlayerContext);
  if (!ctx) throw new Error("usePlayer must be used inside <PlayerProvider>");
  return ctx;
}
