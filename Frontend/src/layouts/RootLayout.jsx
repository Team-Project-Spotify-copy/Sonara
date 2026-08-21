import React from "react";
import { Outlet } from "react-router-dom";
import PlayerBar from "../components/player/PlayerBar.jsx";
import QueueDrawer from "../components/player/QueueDrawer.jsx";
import { usePlayer } from "../contexts/player.store";
import "../css/RootLayout.css";

export default function RootLayout() {
  const { hasStarted, currentTrack, viewMode } = usePlayer();

  const playerVisible = hasStarted && Boolean(currentTrack);

  return (
    <div
      className={`app-root${playerVisible ? " app-root--with-player" : ""}${
        playerVisible && viewMode !== "normal" ? " app-root--immersive" : ""
      }`}
    >
      <main className="app-root__content">
        <Outlet />
      </main>

      <PlayerBar />
      <QueueDrawer />
    </div>
  );
}
