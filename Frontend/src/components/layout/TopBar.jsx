import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchField from "../search/SearchField.jsx";
import { useAccount } from "../../contexts/account.store";
import settingsIcon from "../../assets/icons/settings.svg";
import homeIcon from "../../assets/icons/home.svg";
import searchIcon from "../../assets/icons/search.svg";
import accountIcon from "../../assets/icons/account.svg";
import "../../css/TopBar.css";
import "../../css/auth.css";

/**
 * Figma 707:3955 - identical across all three Home states, so it is shared
 * chrome rather than per-state UI. Settings on the left, home + search pill in
 * the middle, account on the right; all three controls use the warm glass fill.
 */
export default function TopBar({
  query,
  onQueryChange,
  avatarUrl,
  onMenuClick,
  onProfileClick,
}) {
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
        className="topbar__control topbar__control--settings"
        aria-label="Settings"
        onClick={onMenuClick}
      >
        <img src={settingsIcon} alt="" aria-hidden="true" width="24" height="24" />
      </button>

      <div className="topbar__center">
        <button
          type="button"
          className="topbar__control topbar__control--home"
          aria-label="Home"
          onClick={() => navigate("/")}
        >
          <img src={homeIcon} alt="" aria-hidden="true" width="24" height="24" />
        </button>

        <div className="topbar__search">
          <SearchField value={query} onChange={onQueryChange} />
          <img
            className="topbar__search-icon"
            src={searchIcon}
            alt=""
            aria-hidden="true"
            width="24"
            height="24"
          />
        </div>
      </div>

      <div className="topbar__account" ref={menuRef}>
        <button
          type="button"
          className="topbar__control topbar__control--account"
          aria-label={isAuthenticated ? "Account menu" : "Sign in"}
          aria-haspopup={isAuthenticated ? "menu" : undefined}
          aria-expanded={isAuthenticated ? menuOpen : undefined}
          disabled={isLoading}
          onClick={handleAccountClick}
        >
          {avatarUrl ? (
            <span
              className="topbar__avatar-image"
              style={{ backgroundImage: `url(${avatarUrl})` }}
            />
          ) : (
            <img src={accountIcon} alt="" aria-hidden="true" width="24" height="24" />
          )}
        </button>

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
