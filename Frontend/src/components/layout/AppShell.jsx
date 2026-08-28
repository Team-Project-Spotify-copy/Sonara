import backdropUrl from "../../assets/images/login-bg.png";
import "../../css/tokens.css";
import "../../css/AppShell.css";

export default function AppShell({
  topBar,
  rail,
  player,
  children,
  showBackdrop = true,
  showMain = true,
  style,
}) {
  return (
    <div className={`app-shell${player ? " app-shell--with-player" : ""}`}>
      <header className="app-shell__topbar">{topBar}</header>
      <aside className="app-shell__rail">{rail}</aside>

      {showMain ? (
        <main className="app-shell__main">
          {showBackdrop && (
            <img
              className="app-shell__backdrop"
              src={backdropUrl}
              alt=""
              aria-hidden="true"
            />
          )}
          <div style={style} className="app-shell__scroll">
            {children}
          </div>
        </main>
      ) : (
        <>
          {showBackdrop && (
            <img
              className="app-shell__backdrop"
              src={backdropUrl}
              alt=""
              aria-hidden="true"
            />
          )}
          <div style={style} className="app-shell__scroll">
            {children}
          </div>
        </>
      )}

      {player && <footer className="app-shell__player">{player}</footer>}
    </div>
  );
}
