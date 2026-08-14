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

/**
 * Єдиний стан плеєра на весь застосунок. Тут живе один <audio>-елемент;
 * компоненти ніколи не створюють власний і не тримають паралельного стану.
 *
 * Провайдер монтується в App ВИЩЕ за <Routes>, тому зміна маршруту
 * не розмонтовує його і відтворення не переривається.
 *
 * Сам контекст, константи і хук usePlayer — у ./player.store, щоб цей модуль
 * експортував РІВНО один компонент (вимога Fast Refresh).
 */

/**
 * Наскільки раніше за офіційний ExpiresAt вважаємо SAS-посилання протухлим.
 * Бекенд уже віддає ExpiresAt із запасом у хвилину (TrackStreamService), цей
 * запас — другий рубіж на випадок розбіжності годинників клієнта.
 */
const SAS_REFRESH_MARGIN_MS = 30_000;

export function PlayerProvider({ children }) {
  const audioRef = useRef(null);
  if (audioRef.current === null && typeof Audio !== "undefined") {
    audioRef.current = new Audio();
  }

  const [queue, setQueue] = useState([]);
  const [index, setIndex] = useState(0);

  // hasStarted розрізняє «ще нічого не грали» і «плеєр активний».
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

  // Скільки реально прослухано — накопичуємо самі, бо перемотування
  // зробило б currentTime неправдивою метрикою для /listen.
  const listenedMsRef = useRef(0);
  const lastTickRef = useRef(null);
  const reportedForRef = useRef(null);

  // seekbackward/seekforward рахуються від поточної позиції, але перевішувати
  // обробники MediaSession на кожен tick timeupdate не можна — тримаємо позицію в ref.
  const currentTimeRef = useRef(0);

  /**
   * Розв'язане джерело відтворення для ОДНОГО треку — того, що звучить зараз.
   * Черга ніколи не тримає підписаних посилань: SAS живе обмежений час, і
   * пакетне розв'язання під час наповнення черги гарантовано протухло б
   * задовго до того, як користувач дійде до останнього треку.
   */
  const streamRef = useRef({ trackId: null, expiresAt: 0 });

  const isAuthenticated = Boolean(getAccessToken());

  const stopTicking = useCallback(() => {
    if (lastTickRef.current !== null) {
      listenedMsRef.current += Date.now() - lastTickRef.current;
      lastTickRef.current = null;
    }
  }, []);

  /** Надсилає /listen один раз на відтворення. Короткі/повторні події бекенд відхилить сам. */
  const reportListen = useCallback((trackId) => {
    if (!trackId || reportedForRef.current === trackId) return;

    const listened = listenedMsRef.current;
    if (listened <= 0 || !getAccessToken()) return;

    reportedForRef.current = trackId;
    registerListen(trackId, listened).catch(() => {
      // Історія прослуховувань не критична для відтворення — мовчки ігноруємо.
    });
  }, []);

  /**
   * Те саме, але для моменту, коли сторінка вже закривається: звичайний XHR
   * браузер скасує разом із документом, тому йдемо через keepalive-транспорт.
   */
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

  /**
   * Ліниве розв'язання джерела: викликається РІВНО перед відтворенням, а не під
   * час наповнення черги. Повторний виклик для того самого треку мережу не чіпає,
   * доки не наблизився строк дії підписаного посилання.
   */
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

  /**
   * Готує трек до відтворення БЕЗ мережі: метадані з каталогу вже є, а джерело
   * буде розв'язане в ensureStream. Знімаємо старий src, щоб протухле посилання
   * попереднього треку не могло стартувати випадково.
   */
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

  /**
   * Трек передається явно: goTo/setQueueAndPlay викликають відтворення в тому ж
   * такті, у якому щойно зробили setIndex, тож покладатися на currentTrack зі
   * стану не можна — він оновиться лише на наступному рендері.
   */
  const playTrack = useCallback(
    async (track) => {
      const audio = audioRef.current;
      if (!audio || !track) return;

      setError(null);

      // hasStream === false означає, що медіа немає — /stream поверне 409.
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

      // Без autoplay мережевого запиту не буде взагалі: джерело розв'яжеться
      // лише тоді, коли користувач справді натисне «play».
      if (autoplay) void playTrack(track);
    },
    [currentTrackId, playTrack, queue, reportListen, selectTrack, stopTicking],
  );

  const next = useCallback(() => goTo(pickNextIndex()), [goTo, pickNextIndex]);

  const previous = useCallback(() => {
    const audio = audioRef.current;

    // Як у звичних плеєрах: у межах перших 3 секунд «назад» — це попередній трек,
    // далі — перезапуск поточного.
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

  /** Ставить чергу і (за потреби) одразу вмикає потрібний трек. */
  const setQueueAndPlay = useCallback(
    (tracks, startIndex = 0, { autoplay = false } = {}) => {
      setQueue(tracks);
      setIndex(startIndex);
      selectTrack(tracks[startIndex] ?? null);

      if (autoplay && tracks[startIndex]) void playTrack(tracks[startIndex]);
    },
    [playTrack, selectTrack],
  );

  /**
   * Прибирає трек із черги. Індекс поточного треку зсувається так, щоб грати
   * продовжував ТОЙ САМИЙ трек; якщо ж видаляють саме його — переходимо далі.
   */
  const removeFromQueue = useCallback(
    (targetIndex) => {
      if (targetIndex < 0 || targetIndex >= queue.length) return;

      const nextQueue = queue.filter((_, i) => i !== targetIndex);
      setQueue(nextQueue);

      // Прибрали трек ПЕРЕД активним — активний просто зсунувся на позицію лівіше.
      if (targetIndex < index) {
        setIndex(index - 1);
        return;
      }

      // Прибрали трек ПІСЛЯ активного — позиція активного не змінилась.
      if (targetIndex > index) return;

      // Прибрали сам активний трек: його місце посідає наступний.
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

  /** Перетягування в черзі. Активний трек лишається активним, хай куди він переїхав. */
  const moveInQueue = useCallback(
    (from, to) => {
      if (from === to || from < 0 || to < 0 || from >= queue.length || to >= queue.length) {
        return;
      }

      const nextQueue = [...queue];
      const [moved] = nextQueue.splice(from, 1);
      nextQueue.splice(to, 0, moved);
      setQueue(nextQueue);

      // Індекси рахуються від уже наявних queue/index, а не всередині setQueue:
      // вкладені оновлювачі у StrictMode виконуються двічі й зсув застосувався б двічі.
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

    // Оптимістичне оновлення: кнопка реагує миттєво, сервер лишається джерелом істини.
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
        /* лишаємо оптимістичне значення */
      }
    }
  }, [currentTrack?.isLiked, currentTrackId, likeState?.isLiked]);

  // Підписки на події <audio>: єдине місце, де стан плеєра синхронізується з елементом.
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
      // Порожній src — це не збій відтворення: браузер кидає error, коли джерело
      // знімають (напр. під час подвійного монтування у StrictMode).
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

  // «ended» тримаємо окремо: обробник залежить від режиму повтору й черги.
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

  // Стан «вподобано» для поточного треку.
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

  // ── MediaSession: системні медіаклавіші, замок екрана, панель ОС ──────────

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
        // Браузер не підтримує цю дію — решта обробників лишається робочою.
      }
    }

    return () => {
      for (const [action] of handlers) {
        try {
          navigator.mediaSession.setActionHandler(action, null);
        } catch {
          /* нічого знімати */
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

    // Позиція не може перевищувати тривалість — інакше специфікація вимагає кинути.
    if (!Number.isFinite(duration) || duration <= 0) return;

    try {
      navigator.mediaSession.setPositionState({
        duration,
        playbackRate: audioRef.current?.playbackRate ?? 1,
        position: Math.min(currentTime, duration),
      });
    } catch {
      /* нестабільні значення під час перемикання треку */
    }
  }, [currentTime, duration]);

  // ── Телеметрія на закритті вкладки ───────────────────────────────────────

  useEffect(() => {
    // pagehide спрацьовує і при закритті, і при переході у bfcache;
    // visibilitychange -> hidden ловить згортання застосунку на мобільних,
    // де pagehide може не прийти взагалі.
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
        // removeAttribute, а не src = "": порожній рядок резолвиться в адресу
        // сторінки, браузер намагається її завантажити і кидає помилку медіа.
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
