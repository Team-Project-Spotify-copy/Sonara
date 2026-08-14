import React from "react";
import { Outlet } from "react-router-dom";
import PlayerBar from "../components/player/PlayerBar.jsx";
import QueueDrawer from "../components/player/QueueDrawer.jsx";
import { usePlayer } from "../contexts/player.store";
import "../css/RootLayout.css";

/**
 * Оболонка застосунку. <Outlet /> міняється на кожному маршруті, а плеєр —
 * ні: він рендериться тут, СЕСТРОЮ до Outlet, тому React зберігає його
 * піддерево між переходами і <audio> не перестворюється.
 */
export default function RootLayout() {
  const { hasStarted, currentTrack, viewMode } = usePlayer();

  // Панель займає місце внизу лише тоді, коли справді щось грає.
  const playerVisible = hasStarted && Boolean(currentTrack);

  return (
    <div
      className={`app-shell${playerVisible ? " app-shell--with-player" : ""}${
        playerVisible && viewMode !== "normal" ? " app-shell--immersive" : ""
      }`}
    >
      <main className="app-content">
        <Outlet />
      </main>

      <PlayerBar />
      <QueueDrawer />
    </div>
  );
}
