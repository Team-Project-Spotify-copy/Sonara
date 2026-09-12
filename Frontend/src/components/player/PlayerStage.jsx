import React from "react";
import Glyph from "./Glyph.jsx";
import ambientBackdrop from "../../assets/player/rectangle-94.png";
import grainOverlay from "../../assets/player/rectangle-50-tile.png";

/**
 * Figma 661:3134 / 661:3167 - the immersive stage. It fills the shell's main
 * region (gutters left, top bar above, player bar below), holds a blurred
 * ambient wash behind a 580px cover, and drops to the title/artist-only state
 * once the chrome hides.
 *
 * The wash is the exported Rectangle 94 backdrop, which already carries the
 * blur and grain of the design.
 */
export default function PlayerStage({
  artwork,
  title,
  artistName,
  showMeta,
  onCollapse,
}) {
  return (
    <>
      <div className="player-stage">
        <div className="player-stage__surface">
          <img className="player-stage__ambient" src={ambientBackdrop} alt="" />
          <div
            className="player-stage__grain"
            style={{ backgroundImage: `url(${grainOverlay})` }}
          />
        </div>

        <button
          type="button"
          className="player-stage__close"
          onClick={onCollapse}
          aria-label="Exit full screen"
        >
          <Glyph name="collapse" />
        </button>

        <div
          className="player-stage__art"
          style={artwork ? { backgroundImage: `url(${artwork})` } : undefined}
          role="img"
          aria-label={title ? `${title} — ${artistName}` : "Cover art"}
        />
      </div>

      {showMeta && (
        <div className="player-stage__meta">
          <p className="player-stage__title">{title}</p>
          <p className="player-stage__artist">{artistName}</p>
        </div>
      )}
    </>
  );
}
