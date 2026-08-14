import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  getLikeState,
  getTrackStream,
  registerListen,
  sendListenBeacon,
  setLiked as setLikedRequest,
} from "../api/music";
import { errorCode, getAccessToken } from "../api/client";
import { PlayerContext, REPEAT_MODES } from "./player.store";

const SAS_REFRESH_MARGIN_MS = 30_000;

export function PlayerProvider({ children }) {
  const audioRef = useRef(null);
  if (audioRef.current === null && typeof Audio !== "undefined") {
    audioRef.current = new Audio();
  }

  const [queue, setQueue] = useState([]);
  const [index, setIndex] = useState(0);

  const [hasStarted, setHasStarted] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [isLoadingStream, setIsLoadingStream] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [volume, setVolumeState] = useState(1);
  const [muted, setMuted] = useState(false);
  const [shuffle, setShuffle] = useState(false);
  const [repeat, setRepeat] = useState("off");
  const [likeState, setLikeState] = useState(null);
  const [error, setError] = useState(null);

  const [viewMode, setViewMode] = useState("normal");
  const [queueOpen, setQueueOpen] = useState(false);

  const currentTrack = queue[index] ?? null;
  const currentTrackId = currentTrack?.id ?? null;

  const listenedMsRef = useRef(0);
  const lastTickRef = useRef(null);
  const reportedForRef = useRef(null);

  const currentTimeRef = useRef(0);

  const streamRef = useRef({ trackId: null, expiresAt: 0 });

  const isAuthenticated = Boolean(getAccessToken());

  const stopTicking = useCallback(() => {
    if (lastTickRef.current !== null) {
      listenedMsRef.current += Date.now() - lastTickRef.current;
      lastTickRef.current = null;
    }
  }, []);

  const reportListen = useCallback((trackId) => {
    if (!trackId || reportedForRef.current === trackId) return;

    const listened = listenedMsRef.current;
    if (listened <= 0 || !getAccessToken()) return;

    reportedForRef.current = trackId;
    registerListen(trackId, listened).catch(() => {
    });
  }, []);

  const flushListen = useCallback(
    (trackId) => {
      stopTicking();

      if (!trackId || reportedForRef.current === trackId) return;

      const listened = listenedMsRef.current;
      if (listened <= 0 || !getAccessToken()) return;

      reportedForRef.current = trackId;
      sendListenBeacon(trackId, listened);
    },
    [stopTicking],
  );

  const resetListenCounters = useCallback(() => {
    listenedMsRef.current = 0;
    lastTickRef.current = null;
    reportedForRef.current = null;
  }, []);

  const describeError = useCallback((err) => {
    switch (errorCode(err)) {
      case "media_unavailable":
        return "This track has no audio yet.";
      case "unauthorized":
        return "Sign in to play this track.";
      case "storage_unavailable":
        return "Playback is temporarily unavailable. Please try again.";
      default:
        return "Could not start playback.";
    }
  }, []);

  const ensureStream = useCallback(async (track) => {
    const audio = audioRef.current;
    if (!audio || !track) return false;

    const cached = streamRef.current;
    const stillFresh =
      cached.trackId === track.id &&
      Boolean(audio.getAttribute("src")) &&
      (cached.expiresAt === 0 || cached.expiresAt - Date.now() > SAS_REFRESH_MARGIN_MS);

    if (stillFresh) return true;

    setIsLoadingStream(true);
    try {
      const stream = await getTrackStream(track.id);

      streamRef.current = {
        trackId: track.id,
        expiresAt: stream.expiresAt ? Date.parse(stream.expiresAt) : 0,
      };

      audio.src = stream.url;
      audio.load();

      const seconds = (stream.durationMs ?? track.durationMs ?? 0) / 1000;
      if (seconds > 0) setDuration(seconds);

      return true;
    } finally {
      setIsLoadingStream(false);
    }
  }, []);

  const selectTrack = useCallback(
    (track) => {
      const audio = audioRef.current;

      streamRef.current = { trackId: null, expiresAt: 0 };
      resetListenCounters();
      setCurrentTime(0);
      setError(null);
      setDuration((track?.durationMs ?? 0) / 1000);

      if (audio) {
        audio.pause();
        audio.removeAttribute("src");
      }
    },
    [resetListenCounters],
  );

  const playTrack = useCallback(
    async (track) => {
      const audio = audioRef.current;
      if (!audio || !track) return;

      setError(null);

      if (track.hasStream === false) {
        setError("This track has no audio yet.");
        setIsPlaying(false);
        return;
      }

      if (!getAccessToken()) {
        setError("Sign in to play this track.");
        setIsPlaying(false);
        return;
      }

      try {
        await ensureStream(track);
        await audio.play();

        setHasStarted(true);
        setIsPlaying(true);
        lastTickRef.current = Date.now();
      } catch (err) {
        setIsPlaying(false);
        setError(describeError(err));
      }
    },
    [describeError, ensureStream],
  );

  const play = useCallback(() => playTrack(currentTrack), [currentTrack, playTrack]);

  const pause = useCallback(() => {
    const audio = audioRef.current;
    if (!audio) return;

    audio.pause();
    stopTicking();
    setIsPlaying(false);
  }, [stopTicking]);

  const togglePlay = useCallback(() => {
    if (isPlaying) {
      pause();
    } else {
      void play();
    }
  }, [isPlaying, pause, play]);

  const pickNextIndex = useCallback(() => {
    if (queue.length === 0) return 0;
    if (shuffle && queue.length > 1) {
      let nextIndex = index;
      while (nextIndex === index) {
        nextIndex = Math.floor(Math.random() * queue.length);
      }
      return nextIndex;
    }
    return (index + 1) % queue.length;
  }, [index, queue.length, shuffle]);

  const goTo = useCallback(
    (nextIndex, { autoplay = true } = {}) => {
      const track = queue[nextIndex];
      if (!track) return;

      stopTicking();
      reportListen(currentTrackId);

      setIndex(nextIndex);
      selectTrack(track);

      if (autoplay) void playTrack(track);
    },
    [currentTrackId, playTrack, queue, reportListen, selectTrack, stopTicking],
  );

  const next = useCallback(() => goTo(pickNextIndex()), [goTo, pickNextIndex]);

  const previous = useCallback(() => {
    const audio = audioRef.current;

    if (audio && audio.currentTime > 3) {
      audio.currentTime = 0;
      setCurrentTime(0);
      return;
    }

    goTo((index - 1 + queue.length) % Math.max(1, queue.length));
  }, [goTo, index, queue.length]);

  const seek = useCallback((seconds) => {
    const audio = audioRef.current;
    if (!audio || !Number.isFinite(seconds)) return;

    audio.currentTime = Math.max(0, seconds);
    setCurrentTime(audio.currentTime);
  }, []);

  const setVolume = useCallback((value) => {
    const audio = audioRef.current;
    const clamped = Math.min(1, Math.max(0, value));

    setVolumeState(clamped);
    setMuted(clamped === 0);
    if (audio) audio.volume = clamped;
  }, []);

  const toggleMute = useCallback(() => {
    const audio = audioRef.current;
    setMuted((wasMuted) => {
      if (audio) audio.muted = !wasMuted;
      return !wasMuted;
    });
  }, []);

  const cycleRepeat = useCallback(() => {
    setRepeat((mode) => REPEAT_MODES[(REPEAT_MODES.indexOf(mode) + 1) % REPEAT_MODES.length]);
  }, []);

  const toggleShuffle = useCallback(() => setShuffle((on) => !on), []);

  const setQueueAndPlay = useCallback(
    (tracks, startIndex = 0, { autoplay = false } = {}) => {
      setQueue(tracks);
      setIndex(startIndex);
      selectTrack(tracks[startIndex] ?? null);

      if (autoplay && tracks[startIndex]) void playTrack(tracks[startIndex]);
    },
    [playTrack, selectTrack],
  );

  const removeFromQueue = useCallback(
    (targetIndex) => {
      if (targetIndex < 0 || targetIndex >= queue.length) return;

      const nextQueue = queue.filter((_, i) => i !== targetIndex);
      setQueue(nextQueue);

      if (targetIndex < index) {
        setIndex(index - 1);
        return;
      }

      if (targetIndex > index) return;

      const clamped = Math.min(index, Math.max(0, nextQueue.length - 1));
      setIndex(clamped);
      selectTrack(nextQueue[clamped] ?? null);

      if (nextQueue.length === 0) {
        setIsPlaying(false);
        setHasStarted(false);
      }
    },
    [index, queue, selectTrack],
  );

  const moveInQueue = useCallback(
    (from, to) => {
      if (from === to || from < 0 || to < 0 || from >= queue.length || to >= queue.length) {
        return;
      }

      const nextQueue = [...queue];
      const [moved] = nextQueue.splice(from, 1);
      nextQueue.splice(to, 0, moved);
      setQueue(nextQueue);

      if (index === from) setIndex(to);
      else if (from < index && to >= index) setIndex(index - 1);
      else if (from > index && to <= index) setIndex(index + 1);
    },
    [index, queue],
  );

  const toggleQueue = useCallback(() => setQueueOpen((open) => !open), []);
  const closeQueue = useCallback(() => setQueueOpen(false), []);

  const toggleFullscreen = useCallback(
    () => setViewMode((mode) => (mode === "normal" ? "fullscreen" : "normal")),
    [],
  );

  const toggleLyrics = useCallback(
    () => setViewMode((mode) => (mode === "lyrics" ? "fullscreen" : "lyrics")),
    [],
  );

  const collapsePlayer = useCallback(() => setViewMode("normal"), []);

  const toggleLike = useCallback(async () => {
    if (!currentTrackId || !getAccessToken()) return;

    const nextLiked = !(likeState?.isLiked ?? currentTrack?.isLiked ?? false);

    setLikeState((prev) => ({
      trackId: currentTrackId,
      isLiked: nextLiked,
      likesCount: Math.max(0, (prev?.likesCount ?? 0) + (nextLiked ? 1 : -1)),
      likedAt: nextLiked ? new Date().toISOString() : null,
    }));

    try {
      setLikeState(await setLikedRequest(currentTrackId, nextLiked));
    } catch {
      try {
        setLikeState(await getLikeState(currentTrackId));
      } catch {
      }
    }
  }, [currentTrack?.isLiked, currentTrackId, likeState?.isLiked]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return undefined;

    const onTimeUpdate = () => setCurrentTime(audio.currentTime);
    const onLoadedMetadata = () => {
      if (Number.isFinite(audio.duration)) setDuration(audio.duration);
    };
    const onPlay = () => {
      setIsPlaying(true);
      setHasStarted(true);
      lastTickRef.current = Date.now();
    };
    const onPause = () => {
      stopTicking();
      setIsPlaying(false);
    };
    const onError = () => {
      if (!audio.getAttribute("src")) return;

      stopTicking();
      setIsPlaying(false);
      setError("Could not load the audio stream.");
    };

    audio.addEventListener("timeupdate", onTimeUpdate);
    audio.addEventListener("loadedmetadata", onLoadedMetadata);
    audio.addEventListener("play", onPlay);
    audio.addEventListener("pause", onPause);
    audio.addEventListener("error", onError);

    return () => {
      audio.removeEventListener("timeupdate", onTimeUpdate);
      audio.removeEventListener("loadedmetadata", onLoadedMetadata);
      audio.removeEventListener("play", onPlay);
      audio.removeEventListener("pause", onPause);
      audio.removeEventListener("error", onError);
    };
  }, [stopTicking]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return undefined;

    const onEnded = () => {
      stopTicking();
      reportListen(currentTrackId);

      if (repeat === "one") {
        audio.currentTime = 0;
        void audio.play();
        return;
      }

      const isLast = index === queue.length - 1;
      if (isLast && repeat === "off" && !shuffle) {
        setIsPlaying(false);
        return;
      }

      goTo(pickNextIndex());
    };

    audio.addEventListener("ended", onEnded);
    return () => audio.removeEventListener("ended", onEnded);
  }, [
    currentTrackId,
    goTo,
    index,
    pickNextIndex,
    queue.length,
    repeat,
    reportListen,
    shuffle,
    stopTicking,
  ]);

  useEffect(() => {
    if (!currentTrackId) {
      setLikeState(null);
      return undefined;
    }

    if (!getAccessToken()) {
      setLikeState({
        trackId: currentTrackId,
        isLiked: false,
        likesCount: 0,
        likedAt: null,
      });
      return undefined;
    }

    let cancelled = false;
    getLikeState(currentTrackId)
      .then((state) => {
        if (!cancelled) setLikeState(state);
      })
      .catch(() => {
        if (!cancelled) setLikeState(null);
      });

    return () => {
      cancelled = true;
    };
  }, [currentTrackId]);

  useEffect(() => {
    if (typeof navigator === "undefined" || !("mediaSession" in navigator)) return;

    if (!currentTrack) {
      navigator.mediaSession.metadata = null;
      return;
    }

    const artwork = currentTrack.artworkUrl
      ? [
          { src: currentTrack.artworkUrl, sizes: "512x512", type: "image/jpeg" },
          { src: currentTrack.artworkUrl, sizes: "96x96", type: "image/jpeg" },
        ]
      : [];

    navigator.mediaSession.metadata = new window.MediaMetadata({
      title: currentTrack.title ?? "",
      artist: currentTrack.artistName ?? "",
      album: currentTrack.albumTitle ?? "",
      artwork,
    });
  }, [currentTrack]);

  useEffect(() => {
    if (typeof navigator === "undefined" || !("mediaSession" in navigator)) return undefined;

    const handlers = [
      ["play", () => void play()],
      ["pause", () => pause()],
      ["previoustrack", () => previous()],
      ["nexttrack", () => next()],
      ["seekto", (details) => {
        if (details.fastSeek && audioRef.current?.fastSeek) {
          audioRef.current.fastSeek(details.seekTime);
          setCurrentTime(details.seekTime);
          return;
        }
        seek(details.seekTime);
      }],
      ["seekbackward", (details) => seek(currentTimeRef.current - (details.seekOffset || 10))],
      ["seekforward", (details) => seek(currentTimeRef.current + (details.seekOffset || 10))],
      ["stop", () => pause()],
    ];

    for (const [action, handler] of handlers) {
      try {
        navigator.mediaSession.setActionHandler(action, handler);
      } catch {
      }
    }

    return () => {
      for (const [action] of handlers) {
        try {
          navigator.mediaSession.setActionHandler(action, null);
        } catch {
        }
      }
    };
  }, [next, pause, play, previous, seek]);

  useEffect(() => {
    currentTimeRef.current = currentTime;
  }, [currentTime]);

  useEffect(() => {
    if (typeof navigator === "undefined" || !("mediaSession" in navigator)) return;
    navigator.mediaSession.playbackState = isPlaying ? "playing" : "paused";
  }, [isPlaying]);

  useEffect(() => {
    if (typeof navigator === "undefined" || !("mediaSession" in navigator)) return;
    if (typeof navigator.mediaSession.setPositionState !== "function") return;

    if (!Number.isFinite(duration) || duration <= 0) return;

    try {
      navigator.mediaSession.setPositionState({
        duration,
        playbackRate: audioRef.current?.playbackRate ?? 1,
        position: Math.min(currentTime, duration),
      });
    } catch {
    }
  }, [currentTime, duration]);

  useEffect(() => {
    const onPageHide = () => flushListen(currentTrackId);
    const onVisibilityChange = () => {
      if (document.visibilityState === "hidden") flushListen(currentTrackId);
    };

    window.addEventListener("pagehide", onPageHide);
    document.addEventListener("visibilitychange", onVisibilityChange);

    return () => {
      window.removeEventListener("pagehide", onPageHide);
      document.removeEventListener("visibilitychange", onVisibilityChange);
      flushListen(currentTrackId);
    };
  }, [currentTrackId, flushListen]);

  useEffect(() => {
    const audio = audioRef.current;
    return () => {
      if (audio) {
        audio.pause();
        audio.removeAttribute("src");
      }
    };
  }, []);

  const value = useMemo(
    () => ({
      queue,
      index,
      currentTrack,
      nextTrack: queue[(index + 1) % Math.max(1, queue.length)] ?? null,
      hasStarted,
      isPlaying,
      isLoadingStream,
      currentTime,
      duration,
      volume,
      muted,
      shuffle,
      repeat,
      likeState,
      isLiked: likeState?.isLiked ?? currentTrack?.isLiked ?? false,
      error,
      isAuthenticated,
      viewMode,
      queueOpen,
      setQueueAndPlay,
      play,
      pause,
      togglePlay,
      next,
      previous,
      goTo,
      seek,
      setVolume,
      toggleMute,
      toggleShuffle,
      cycleRepeat,
      toggleLike,
      removeFromQueue,
      moveInQueue,
      toggleQueue,
      closeQueue,
      setViewMode,
      toggleFullscreen,
      toggleLyrics,
      collapsePlayer,
    }),
    [
      queue,
      index,
      currentTrack,
      hasStarted,
      isPlaying,
      isLoadingStream,
      currentTime,
      duration,
      volume,
      muted,
      shuffle,
      repeat,
      likeState,
      error,
      isAuthenticated,
      viewMode,
      queueOpen,
      setQueueAndPlay,
      play,
      pause,
      togglePlay,
      next,
      previous,
      goTo,
      seek,
      setVolume,
      toggleMute,
      toggleShuffle,
      cycleRepeat,
      toggleLike,
      removeFromQueue,
      moveInQueue,
      toggleQueue,
      closeQueue,
      toggleFullscreen,
      toggleLyrics,
      collapsePlayer,
    ],
  );

  return <PlayerContext.Provider value={value}>{children}</PlayerContext.Provider>;
}
