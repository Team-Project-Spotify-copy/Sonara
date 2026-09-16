import "@css/tokens.css";
import "@css/AppShell.css";

export default function AppShell({
  topBar,
  rail,
  player,
  children,
  showMain = true,
  showRail = true,
  railExpanded = false,
  style,
}) {
  const hasRail = showRail && rail;
  return (
    <div
      className={`app-shell ${!hasRail ? "app-shell--no-rail" : ""}${player ? " app-shell--with-player" : ""}${
        hasRail && railExpanded ? " app-shell--rail-expanded" : ""
      }`}
    >
      <header className="app-shell__topbar">{topBar}</header>
      {hasRail && <aside className="app-shell__rail">{rail}</aside>}

      {showMain ? (
        <main className="app-shell__main">
          <div style={style} className="app-shell__scroll">
            {children}
          </div>
        </main>
      ) : (
        <>
          <div style={style} className="app-shell__scroll">
            {children}
          </div>
        </>
      )}
      {player && <footer className="app-shell__player">{player}</footer>}
    </div>
  );
}
