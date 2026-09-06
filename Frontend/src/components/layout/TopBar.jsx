import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchField from "../search/SearchField.jsx";
import { useAccount } from "../../contexts/account.store";
import "../../css/TopBar.css";
import "../../css/auth.css";

export default function TopBar({ query, onQueryChange, avatarUrl, onMenuClick, onProfileClick }) {
  const { isAuthenticated, isLoading, username, email, logout } = useAccount();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const [signingOut, setSigningOut] = useState(false);
  const menuRef = useRef(null);

  useEffect(() => {
    if (!menuOpen) return undefined;

    const onPointerDown = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target)) setMenuOpen(false);
    };
    const onKeyDown = (event) => {
      if (event.key === "Escape") setMenuOpen(false);
    };

    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [menuOpen]);

  const handleAccountClick = () => {
    if (onProfileClick) {
      onProfileClick();
      return;
    }
    if (isAuthenticated) setMenuOpen((open) => !open);
    else navigate("/login");
  };

  const handleLogout = async () => {
    setSigningOut(true);
    try {
      await logout();
      setMenuOpen(false);
      navigate("/login", { replace: true });
    } finally {
      setSigningOut(false);
    }
  };

  return (
    <div className="topbar">
      <button
        type="button"
        className="topbar__avatar"
        aria-label="Menu"
        onClick={onMenuClick}
      />
      <div className="topbar__search">
        <SearchField value={query} onChange={onQueryChange} />
      </div>

      <div className="topbar__account" ref={menuRef}>
        <button
          type="button"
          className="topbar__avatar"
          aria-label={isAuthenticated ? "Account menu" : "Sign in"}
          aria-haspopup={isAuthenticated ? "menu" : undefined}
          aria-expanded={isAuthenticated ? menuOpen : undefined}
          style={avatarUrl ? { backgroundImage: `url(${avatarUrl})` } : undefined}
          disabled={isLoading}
          onClick={handleAccountClick}
        />

        {menuOpen && isAuthenticated && (
          <div className="topbar__menu" role="menu">
            <p className="topbar__menu-identity">{username || email || "Signed in"}</p>
            <button
              type="button"
              role="menuitem"
              className="topbar__menu-item"
              onClick={() => {
                setMenuOpen(false);
                navigate("/account");
              }}
            >
              Account
            </button>
            <button
              type="button"
              role="menuitem"
              className="topbar__menu-item"
              onClick={handleLogout}
              disabled={signingOut}
            >
              {signingOut ? "Signing out…" : "Log out"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
