import React from "react";
import { Outlet } from "react-router-dom";
import PlayerBar from "../components/player/PlayerBar.jsx";
import QueueDrawer from "../components/player/QueueDrawer.jsx";
import LyricsPanel from "../components/player/LyricsPanel.jsx";
import { usePlayer } from "../contexts/player.store";
import "../css/RootLayout.css";

export default function RootLayout() {
  const { hasStarted, currentTrack, viewMode, queueOpen, lyricsOpen } = usePlayer();

  const playerVisible = hasStarted && Boolean(currentTrack);
  const panelOpen = playerVisible && (queueOpen || lyricsOpen);

  // The shell reads both as CSS variables: the bar gets its own row instead of
  // overlapping the scroll area, and an open panel narrows the main panel the
  // way Figma 661:2827 does.
  const shellVars = {
    "--player-offset": playerVisible ? "var(--player-height)" : "0px",
    "--side-panel": panelOpen
      ? "calc(var(--side-panel-width) + var(--shell-gap))"
      : "0px",
  };

  return (
    <div
      className={`app-root${playerVisible ? " app-root--with-player" : ""}${
        playerVisible && viewMode === "fullscreen" ? " app-root--immersive" : ""
      }`}
      style={shellVars}
    >
      <main className="app-root__content">
        <Outlet />
      </main>

      <PlayerBar />
      <LyricsPanel />
      <QueueDrawer />
    </div>
  );
}
