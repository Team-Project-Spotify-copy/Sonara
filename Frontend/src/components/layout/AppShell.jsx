import backdropUrl from "../../assets/images/login-bg.png";
import "../../css/tokens.css";
import "../../css/AppShell.css";

export default function AppShell({ topBar, rail, player, children }) {
  return (
    <div className={`app-shell${player ? " app-shell--with-player" : ""}`}>
      <header className="app-shell__topbar">{topBar}</header>
      <aside className="app-shell__rail">{rail}</aside>

      <main className="app-shell__main">
        <img className="app-shell__backdrop" src={backdropUrl} alt="" aria-hidden="true" />
        <div className="app-shell__scroll">{children}</div>
      </main>

      {player && <footer className="app-shell__player">{player}</footer>}
    </div>
  );
}
