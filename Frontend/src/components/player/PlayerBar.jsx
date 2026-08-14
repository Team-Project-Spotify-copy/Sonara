import { usePlayer } from "../../contexts/player.context.jsx";
import "../../css/PlayerBar.css";

function Icon({ d, size = 18 }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <path d={d} />
    </svg>
  );
}

const PATHS = {
  play: "M8 5v14l11-7z",
  pause: "M6 5h4v14H6zm8 0h4v14h-4z",
  prev: "M6 6h2v12H6zm3 6l9 6V6z",
  next: "M16 6h2v12h-2zM6 18l9-6-9-6z",
  shuffle:
    "M17 3l4 4-4 4V8h-2.5l-9 9H3v-2h2.5l9-9H17V3zM3 7h2.5l2.7 2.7-1.4 1.4L4.7 9H3V7zm12.6 6.9L17 12.5l4 4-4 4V18h-2.5l-3.2-3.2 1.4-1.4 2.6 2.6H17v-2.1z",
  repeat: "M7 7h10v3l4-4-4-4v3H5v6h2V7zm10 10H7v-3l-4 4 4 4v-3h12v-6h-2v4z",
  queue: "M3 6h13v2H3zm0 5h13v2H3zm0 5h9v2H3zm15-9l4 3-4 3z",
  close: "M18.3 5.7l-1.4-1.4-4.9 4.9-4.9-4.9-1.4 1.4 4.9 4.9-4.9 4.9 1.4 1.4 4.9-4.9 4.9 4.9 1.4-1.4-4.9-4.9z",
  volume: "M5 9v6h4l5 4V5L9 9H5zm11.5 3a4.5 4.5 0 00-2.5-4v8a4.5 4.5 0 002.5-4z",
  muted:
    "M5 9v6h4l5 4V5L9 9H5zm14.5 3l2.5 2.5-1 1L18.5 13 16 15.5l-1-1L17.5 12 15 9.5l1-1 2.5 2.5L21 8.5l1 1L19.5 12z",
};

function formatTime(seconds) {
  if (!Number.isFinite(seconds) || seconds < 0) return "0:00";
  const minutes = Math.floor(seconds / 60);
  const rest = Math.floor(seconds % 60);
  return `${minutes}:${String(rest).padStart(2, "0")}`;
}

export default function PlayerBar() {
  const {
    currentTrack,
    isPlaying,
    position,
    duration,
    muted,
    shuffle,
    repeat,
    toggle,
    stop,
    next,
    previous,
    seek,
    toggleMute,
    toggleShuffle,
    cycleRepeat,
  } = usePlayer();

  const max = duration || currentTrack?.durationSeconds || 0;
  const disabled = !currentTrack;
  const percent = max ? (Math.min(position, max) / max) * 100 : 0;

  return (
    <div className="player">
      <div className="player__now">
        <span
          className="player__art"
          style={
            currentTrack?.imageUrl ? { backgroundImage: `url(${currentTrack.imageUrl})` } : undefined
          }
        />
        <span className="player__meta">
          <span className="player__title">{currentTrack?.title ?? "Name"}</span>
          <span className="player__artist">{currentTrack?.subtitle ?? "Artist"}</span>
        </span>
      </div>

      <div className="player__transport">
        <div className="player__buttons">
          <button
            type="button"
            className="player__btn"
            aria-label="Previous"
            onClick={previous}
            disabled={disabled}
          >
            <Icon d={PATHS.prev} />
          </button>
          <button
            type="button"
            className="player__btn player__btn--primary"
            aria-label={isPlaying ? "Pause" : "Play"}
            onClick={toggle}
            disabled={disabled}
          >
            <Icon d={isPlaying ? PATHS.pause : PATHS.play} size={22} />
          </button>
          <button
            type="button"
            className="player__btn"
            aria-label="Next"
            onClick={next}
            disabled={disabled}
          >
            <Icon d={PATHS.next} />
          </button>
        </div>

        <div className="player__scrubber">
          <span className="player__time">{formatTime(position)}</span>
          <input
            type="range"
            className="player__range"
            min={0}
            max={max || 100}
            step={1}
            value={Math.min(position, max || 0)}
            disabled={disabled || !max}
            aria-label="Seek"
            onChange={(event) => seek(Number(event.target.value))}
            style={{
              background: `linear-gradient(to right, var(--color-accent) ${percent}%, var(--color-surface-raised) ${percent}%)`,
            }}
          />
          <span className="player__time">{formatTime(max)}</span>
        </div>
      </div>

      <div className="player__extras">
        <button
          type="button"
          className={`player__btn${shuffle ? " player__btn--active" : ""}`}
          aria-pressed={shuffle}
          aria-label="Shuffle"
          onClick={toggleShuffle}
        >
          <Icon d={PATHS.shuffle} />
        </button>
        <button
          type="button"
          className={`player__btn${repeat !== "off" ? " player__btn--active" : ""}`}
          aria-label={`Repeat: ${repeat}`}
          onClick={cycleRepeat}
        >
          <Icon d={PATHS.repeat} />
        </button>
        <button type="button" className="player__btn" aria-label="Queue">
          <Icon d={PATHS.queue} />
        </button>
        <button
          type="button"
          className="player__btn"
          aria-label={muted ? "Unmute" : "Mute"}
          onClick={toggleMute}
        >
          <Icon d={muted ? PATHS.muted : PATHS.volume} />
        </button>
        <button
          type="button"
          className="player__btn"
          aria-label="Close player"
          onClick={stop}
        >
          <Icon d={PATHS.close} />
        </button>
      </div>
    </div>
  );
}
