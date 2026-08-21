import React, { useCallback, useMemo, useRef } from "react";
import { usePlayer } from "../../contexts/player.store";
import Glyph from "./Glyph.jsx";
import { formatTime } from "../../utils/time";
import ambientGradient from "../../assets/player/song-ambient.svg";
import grainOverlay from "../../assets/player/rectangle-50-tile.png";
import "../../css/PlayerBar.css";

export default function PlayerBar() {
  const {
    currentTrack,
    hasStarted,
    isPlaying,
    isLoadingStream,
    currentTime,
    duration,
    muted,
    shuffle,
    repeat,
    isLiked,
    error,
    isAuthenticated,
    viewMode,
    queueOpen,
    togglePlay,
    next,
    previous,
    seek,
    toggleMute,
    toggleShuffle,
    cycleRepeat,
    toggleLike,
    toggleQueue,
    toggleFullscreen,
    toggleLyrics,
    collapsePlayer,
  } = usePlayer();

  const progressRef = useRef(null);

  const onSeek = useCallback(
    (event) => {
      const element = progressRef.current;
      if (!element || !duration) return;

      const { left, width } = element.getBoundingClientRect();
      const ratio = Math.min(1, Math.max(0, (event.clientX - left) / width));
      seek(ratio * duration);
    },
    [duration, seek],
  );

  const effectiveDuration = duration || (currentTrack?.durationMs ?? 0) / 1000;

  const progressPercent = useMemo(() => {
    if (!effectiveDuration) return 0;
    return Math.min(100, Math.max(0, (currentTime / effectiveDuration) * 100));
  }, [currentTime, effectiveDuration]);

  if (!hasStarted || !currentTrack) return null;

  const artwork = currentTrack.artworkUrl ?? null;
  const title = currentTrack.title ?? "";
  const artistName = currentTrack.artistName ?? "";
  const immersive = viewMode !== "normal";

  return (
    <div className={`player player--${viewMode}`} data-node-id="324:971">
      {immersive && (
        <div className="player-stage" aria-hidden="true">
          <div className="player-stage-surface" data-node-id="324:929">
            <img className="player-stage-ambient" src={ambientGradient} alt="" />
            <div className="player-stage-blur" />
            <div
              className="player-stage-grain"
              style={{ backgroundImage: `url(${grainOverlay})` }}
              data-node-id="324:951"
            />
          </div>
        </div>
      )}

      {immersive && (
        <div className="player-immersive">
          <div
            className="player-artwork"
            style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
            role="img"
            aria-label={title ? `${title} — ${artistName}` : "Cover art"}
            data-node-id="324:938"
          />

          {viewMode === "lyrics" && (
            <section className="player-lyrics" data-node-id="324:1036">
              <div className="player-lyrics-bg" />
              <div className="player-lyrics-body">
                <p className="player-lyrics-head">{title}</p>
                <p className="player-lyrics-text">
                  {artistName ? `${artistName}\n\n` : ""}
                  Lyrics aren’t available for this track yet.
                </p>
              </div>
              <div className="player-lyrics-fade" />
            </section>
          )}

          {viewMode === "fullscreen" && (
            <div className="player-immersive-meta" data-node-id="324:939">
              <p className="player-immersive-title">{title || "Name"}</p>
              <p className="player-immersive-artist">{artistName || "Artist"}</p>
            </div>
          )}
        </div>
      )}

      <div className="player-bar" data-node-id="324:972">
        {error && <p className="player-error">{error}</p>}

        <div className="player-bar-left">
          <button
            type="button"
            className="player-cover"
            style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
            onClick={immersive ? collapsePlayer : toggleFullscreen}
            aria-label={immersive ? "Exit full screen" : "Open full screen"}
            data-node-id="324:990"
          >
            <span className="player-cover-hint">
              <Glyph name={immersive ? "collapse" : "expand"} />
            </span>
          </button>

          <div className="player-meta" data-node-id="324:991">
            <p className="player-title">{title}</p>
            <p className="player-artist">{artistName}</p>
          </div>
        </div>

        <div className="player-center" data-node-id="324:973">
          <div className="player-transport" data-node-id="324:974">
            <button
              type="button"
              className="player-btn player-btn--sm"
              onClick={previous}
              aria-label="Previous track"
            >
              <Glyph name="prev" />
            </button>
            <button
              type="button"
              className="player-btn player-btn--lg"
              onClick={togglePlay}
              disabled={isLoadingStream}
              aria-label={isPlaying ? "Pause" : "Play"}
            >
              <Glyph name={isPlaying ? "pause" : "play"} />
            </button>
            <button
              type="button"
              className="player-btn player-btn--sm"
              onClick={next}
              aria-label="Next track"
            >
              <Glyph name="next" />
            </button>
          </div>

          <div className="player-progress" data-node-id="324:978">
            <p className="player-time">{formatTime(currentTime)}</p>
            <button
              type="button"
              ref={progressRef}
              className="player-progress-track"
              onClick={onSeek}
              disabled={!effectiveDuration}
              aria-label="Seek"
              data-node-id="324:980"
            >
              <span className="player-progress-fill" style={{ width: `${progressPercent}%` }} />
            </button>
            <p className="player-time">{formatTime(effectiveDuration)}</p>
          </div>
        </div>

        <div className="player-actions" data-node-id="324:984">
          <button
            type="button"
            className={`player-btn player-btn--lg${isLiked ? " player-btn--active" : ""}`}
            onClick={toggleLike}
            disabled={!isAuthenticated}
            aria-pressed={isLiked}
            aria-label={isLiked ? "Remove from favorites" : "Add to favorites"}
          >
            <Glyph name="heart" />
          </button>
          <button
            type="button"
            className={`player-btn player-btn--lg${shuffle ? " player-btn--active" : ""}`}
            onClick={toggleShuffle}
            aria-pressed={shuffle}
            aria-label="Shuffle"
          >
            <Glyph name="shuffle" />
          </button>
          <button
            type="button"
            className={`player-btn player-btn--lg${repeat !== "off" ? " player-btn--active" : ""}`}
            onClick={cycleRepeat}
            aria-label={`Repeat: ${repeat}`}
          >
            <Glyph name="repeat" />
          </button>
          <button
            type="button"
            className={`player-btn player-btn--lg${muted ? " player-btn--active" : ""}`}
            onClick={toggleMute}
            aria-pressed={muted}
            aria-label={muted ? "Unmute" : "Mute"}
          >
            <Glyph name={muted ? "muted" : "volume"} />
          </button>
          <button
            type="button"
            className={`player-btn player-btn--lg${viewMode === "lyrics" ? " player-btn--active" : ""}`}
            onClick={toggleLyrics}
            aria-pressed={viewMode === "lyrics"}
            aria-label="Lyrics"
          >
            <Glyph name="lyrics" />
          </button>
          <button
            type="button"
            className={`player-btn player-btn--lg${queueOpen ? " player-btn--active" : ""}`}
            onClick={toggleQueue}
            aria-pressed={queueOpen}
            aria-label="Up next"
          >
            <Glyph name="queue" />
          </button>
        </div>
      </div>
    </div>
  );
}
