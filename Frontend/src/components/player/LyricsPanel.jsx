import React from "react";
import { usePlayer } from "../../contexts/player.store";
import Glyph from "./Glyph.jsx";
import "../../css/PlayerPanels.css";

/**
 * Figma 661:2827 - the lyrics rail docks beside the main panel and the panel
 * gives up its width; 661:3331 is the same block inside the expanded view.
 *
 * The catalog API exposes no lyrics endpoint, so the body states that rather
 * than standing in fabricated text.
 */
export default function LyricsPanel() {
  const { currentTrack, hasStarted, lyricsOpen, closeLyrics } = usePlayer();

  if (!lyricsOpen || !hasStarted || !currentTrack) return null;

  return (
    <aside className="side-panel side-panel--lyrics" aria-label="Lyrics">
      <header className="side-panel__head">
        <h2 className="side-panel__title">Lyrics</h2>
        <button
          type="button"
          className="side-panel__close"
          onClick={closeLyrics}
          aria-label="Close lyrics"
        >
          <Glyph name="close" />
        </button>
      </header>

      <div className="side-panel__body lyrics-body">
        <p className="lyrics-body__head">{currentTrack.title}</p>
        <p className="lyrics-body__line">{currentTrack.artistName}</p>
        <p className="lyrics-body__line lyrics-body__line--muted">
          Lyrics aren’t available for this track yet.
        </p>
      </div>

      <div className="side-panel__fade" aria-hidden="true" />
    </aside>
  );
}
