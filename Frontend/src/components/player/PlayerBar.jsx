import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { usePlayer } from "../../contexts/player.store";
import Glyph from "./Glyph.jsx";
import PlayerStage from "./PlayerStage.jsx";
import { formatTime } from "../../utils/time";
import "../../css/PlayerBar.css";

/** Time before the immersive view drops its chrome (Figma 661:3134). */
const IDLE_DELAY = 2600;

export default function PlayerBar() {
  const {
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
    isLiked,
    error,
    isAuthenticated,
    viewMode,
    queueOpen,
    lyricsOpen,
    togglePlay,
    next,
    previous,
    seek,
    setVolume,
    toggleMute,
    toggleShuffle,
    cycleRepeat,
    toggleLike,
    toggleQueue,
    toggleLyrics,
    toggleFullscreen,
    collapsePlayer,
  } = usePlayer();

  const progressRef = useRef(null);
  const [scrubRatio, setScrubRatio] = useState(null);
  const [chromeVisible, setChromeVisible] = useState(true);

  const immersive = viewMode === "fullscreen";

  const effectiveDuration = duration || (currentTrack?.durationMs ?? 0) / 1000;

  const ratioFromEvent = useCallback((event) => {
    const element = progressRef.current;
    if (!element) return null;

    const { left, width } = element.getBoundingClientRect();
    if (!width) return null;

    return Math.min(1, Math.max(0, (event.clientX - left) / width));
  }, []);

  const onScrubStart = useCallback(
    (event) => {
      if (!effectiveDuration) return;

      const ratio = ratioFromEvent(event);
      if (ratio === null) return;

      setScrubRatio(ratio);
      event.currentTarget.setPointerCapture?.(event.pointerId);
    },
    [effectiveDuration, ratioFromEvent],
  );

  const onScrubMove = useCallback(
    (event) => {
      if (scrubRatio === null) return;

      const ratio = ratioFromEvent(event);
      if (ratio !== null) setScrubRatio(ratio);
    },
    [ratioFromEvent, scrubRatio],
  );

  const onScrubEnd = useCallback(
    (event) => {
      if (scrubRatio === null) return;

      const ratio = ratioFromEvent(event) ?? scrubRatio;
      setScrubRatio(null);
      event.currentTarget.releasePointerCapture?.(event.pointerId);
      seek(ratio * effectiveDuration);
    },
    [effectiveDuration, ratioFromEvent, scrubRatio, seek],
  );

  const onProgressKeyDown = useCallback(
    (event) => {
      if (!effectiveDuration) return;

      const step = event.shiftKey ? 30 : 5;
      if (event.key === "ArrowRight") seek(Math.min(effectiveDuration, currentTime + step));
      else if (event.key === "ArrowLeft") seek(Math.max(0, currentTime - step));
      else if (event.key === "Home") seek(0);
      else if (event.key === "End") seek(effectiveDuration);
      else return;

      event.preventDefault();
    },
    [currentTime, effectiveDuration, seek],
  );

  const displayedTime = scrubRatio !== null ? scrubRatio * effectiveDuration : currentTime;

  const progressPercent = useMemo(() => {
    if (!effectiveDuration) return 0;
    return Math.min(100, Math.max(0, (displayedTime / effectiveDuration) * 100));
  }, [displayedTime, effectiveDuration]);

  // Full screen hides its chrome while the pointer rests (Figma 661:3134) and
  // brings it back on any input (661:3167).
  useEffect(() => {
    if (!immersive) {
      setChromeVisible(true);
      return undefined;
    }

    let timer = null;

    const schedule = () => {
      setChromeVisible(true);
      window.clearTimeout(timer);
      timer = window.setTimeout(() => setChromeVisible(false), IDLE_DELAY);
    };

    schedule();
    window.addEventListener("pointermove", schedule);
    window.addEventListener("keydown", schedule);

    return () => {
      window.clearTimeout(timer);
      window.removeEventListener("pointermove", schedule);
      window.removeEventListener("keydown", schedule);
    };
  }, [immersive]);

  useEffect(() => {
    if (!immersive) return undefined;

    const onKeyDown = (event) => {
      if (event.key === "Escape") collapsePlayer();
    };

    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [collapsePlayer, immersive]);

  if (!hasStarted || !currentTrack) return null;

  const artwork = currentTrack.artworkUrl ?? null;
  const title = currentTrack.title ?? "";
  const artistName = currentTrack.artistName ?? "";

  return (
    <div
      className={[
        "player",
        immersive ? "player--immersive" : "player--docked",
        immersive && !chromeVisible ? "player--idle" : "",
      ]
        .filter(Boolean)
        .join(" ")}
      data-node-id="845:4302"
    >
      {immersive && (
        <PlayerStage
          artwork={artwork}
          title={title}
          artistName={artistName}
          showMeta={!chromeVisible}
          onCollapse={collapsePlayer}
        />
      )}

      <div className="player-bar">
        {error && <p className="player-bar__error">{error}</p>}

        <div className="player-bar__lead">
          <button
            type="button"
            className="player-cover"
            style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
            onClick={immersive ? collapsePlayer : toggleFullscreen}
            aria-label={immersive ? "Exit full screen" : "Open full screen"}
          >
            <span className="player-cover__hint">
              <Glyph name={immersive ? "collapse" : "expand"} />
            </span>
          </button>

          <div className="player-meta">
            <p className="player-meta__title">{title}</p>
            <p className="player-meta__artist">{artistName}</p>
          </div>

          <button
            type="button"
            className={`player-like${isLiked ? " player-like--on" : ""}`}
            onClick={toggleLike}
            disabled={!isAuthenticated}
            aria-pressed={isLiked}
            aria-label={isLiked ? "Remove from favorites" : "Add to favorites"}
            title={isAuthenticated ? undefined : "Sign in to save tracks"}
          >
            <Glyph name="star" filled={isLiked} />
          </button>
        </div>

        <div className="player-center">
          <div className="player-transport">
            <button
              type="button"
              className="player-btn player-btn--step"
              onClick={previous}
              aria-label="Previous track"
            >
              <Glyph name="prev" />
            </button>
            <button
              type="button"
              className="player-btn player-btn--primary"
              onClick={togglePlay}
              disabled={isLoadingStream}
              aria-label={isPlaying ? "Pause" : "Play"}
            >
              <Glyph name={isPlaying ? "pause" : "play"} />
            </button>
            <button
              type="button"
              className="player-btn player-btn--step"
              onClick={next}
              aria-label="Next track"
            >
              <Glyph name="next" />
            </button>
          </div>

          <div className="player-progress">
            <p className="player-progress__time">{formatTime(displayedTime)}</p>
            <div
              ref={progressRef}
              className="player-progress__track"
              role="slider"
              tabIndex={0}
              aria-label="Seek"
              aria-valuemin={0}
              aria-valuemax={Math.round(effectiveDuration)}
              aria-valuenow={Math.round(displayedTime)}
              aria-valuetext={formatTime(displayedTime)}
              aria-disabled={!effectiveDuration}
              onPointerDown={onScrubStart}
              onPointerMove={onScrubMove}
              onPointerUp={onScrubEnd}
              onPointerCancel={onScrubEnd}
              onKeyDown={onProgressKeyDown}
            >
              <span
                className="player-progress__fill"
                style={{ width: `${progressPercent}%` }}
              />
              <span
                className="player-progress__knob"
                style={{ left: `${progressPercent}%` }}
              />
            </div>
            <p className="player-progress__time">{formatTime(effectiveDuration)}</p>
          </div>
        </div>

        <div className="player-actions">
          <button
            type="button"
            className={`player-btn player-btn--ghost player-btn--lyrics${lyricsOpen ? " player-btn--on" : ""}`}
            onClick={toggleLyrics}
            aria-pressed={lyricsOpen}
            aria-label="Lyrics"
          >
            <Glyph name="lyrics" />
          </button>

          <button
            type="button"
            className={`player-btn player-btn--ghost player-btn--queue${queueOpen ? " player-btn--on" : ""}`}
            onClick={toggleQueue}
            aria-pressed={queueOpen}
            aria-label="Up next"
          >
            <Glyph name="headphones" />
          </button>

          <div className="player-volume">
            <button
              type="button"
              className={`player-btn player-btn--ghost player-btn--volume${muted ? " player-btn--on" : ""}`}
              onClick={toggleMute}
              aria-pressed={muted}
              aria-label={muted ? "Unmute" : "Mute"}
            >
              <Glyph name={muted ? "muted" : "volume"} />
            </button>
            <label className="player-volume__slider">
              <span className="player-volume__label">Volume</span>
              <input
                type="range"
                min="0"
                max="1"
                step="0.01"
                value={muted ? 0 : volume}
                onChange={(event) => setVolume(Number(event.target.value))}
              />
            </label>
          </div>

          <button
            type="button"
            className={`player-btn player-btn--ghost player-btn--shuffle${shuffle ? " player-btn--on" : ""}`}
            onClick={toggleShuffle}
            aria-pressed={shuffle}
            aria-label="Shuffle"
          >
            <Glyph name="shuffle" />
          </button>

          <button
            type="button"
            className={`player-btn player-btn--ghost player-btn--repeat${repeat !== "off" ? " player-btn--on" : ""}`}
            onClick={cycleRepeat}
            aria-label={`Repeat: ${repeat}`}
          >
            <Glyph name={repeat === "one" ? "repeat-one" : "repeat"} />
          </button>

          <button
            type="button"
            className={`player-btn player-btn--ghost player-btn--fullscreen${immersive ? " player-btn--on" : ""}`}
            onClick={immersive ? collapsePlayer : toggleFullscreen}
            aria-pressed={immersive}
            aria-label={immersive ? "Exit full screen" : "Full screen"}
          >
            <Glyph name={immersive ? "collapse" : "expand"} />
          </button>
        </div>
      </div>
    </div>
  );
}
