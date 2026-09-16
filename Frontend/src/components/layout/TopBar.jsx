import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchField from "@components/search/SearchField.jsx";
import SearchResults from "@components/search/SearchResults.jsx";
import { useAccount } from "@contexts/account.store";
import settingsIcon from "@assets/icons/settings.svg";
import homeIcon from "@assets/icons/home.svg";
import searchIcon from "@assets/icons/search.svg";
import accountIcon from "@assets/icons/account.svg";
import "@css/TopBar.css";
import "@css/auth.css";

const PANEL_ID = "topbar-search-panel";

/**
 * Figma 707:3955 - identical across all three Home states, so it is shared
 * chrome rather than per-state UI. Settings on the left, home + search pill in
 * the middle, account on the right; all three controls use the warm glass fill.
 *
 * Figma 686:1212 hangs the search results off the pill as a 424px dropdown, so
 * results never replace the page behind it.
 */
export default function TopBar({
  query,
  onQueryChange,
  searching = false,
  searchResults,
  searchStatus,
  searchError,
  recentSearches = [],
  onClearRecent,
  onSearchSelect,
  avatarUrl,
  onMenuClick,
  onProfileClick,
}) {
  const { isAuthenticated, isLoading, username, email, logout } = useAccount();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [signingOut, setSigningOut] = useState(false);
  const menuRef = useRef(null);
  const settingsRef = useRef(null);
  const searchRef = useRef(null);

  useEffect(() => {
    if (!menuOpen) return undefined;

    const onPointerDown = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target))
        setMenuOpen(false);
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

  useEffect(() => {
    if (!settingsOpen) return undefined;

    const onPointerDown = (event) => {
      if (settingsRef.current && !settingsRef.current.contains(event.target)) {
        setSettingsOpen(false);
      }
    };
    const onKeyDown = (event) => {
      if (event.key === "Escape") setSettingsOpen(false);
    };

    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [settingsOpen]);

  useEffect(() => {
    if (!searchOpen) return undefined;

    const onPointerDown = (event) => {
      if (searchRef.current && !searchRef.current.contains(event.target)) {
        setSearchOpen(false);
      }
    };

    document.addEventListener("pointerdown", onPointerDown);
    return () => document.removeEventListener("pointerdown", onPointerDown);
  }, [searchOpen]);

  // Typing reopens a panel that was dismissed with Escape.
  const handleQueryChange = useCallback(
    (value) => {
      onQueryChange(value);
      setSearchOpen(true);
    },
    [onQueryChange],
  );

  const handleSearchKeyDown = useCallback((event) => {
    if (event.key === "Escape") {
      setSearchOpen(false);
      event.currentTarget.blur();
    }
  }, []);

  const handleSelect = useCallback(
    (item) => {
      setSearchOpen(false);
      onSearchSelect?.(item);
    },
    [onSearchSelect],
  );

  const handleSettingsClick = () => {
    if (onMenuClick) {
      onMenuClick();
      return;
    }
    setSettingsOpen((open) => !open);
  };

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
      <div className="topbar__settings" ref={settingsRef}>
        <button
          type="button"
          className="topbar__control topbar__control--settings"
          aria-label="Settings"
          aria-haspopup="menu"
          aria-expanded={settingsOpen}
          onClick={handleSettingsClick}
        >
          <img
            src={settingsIcon}
            alt=""
            aria-hidden="true"
            width="24"
            height="24"
          />
        </button>

        {settingsOpen && (
          <div className="topbar__menu topbar__menu--settings" role="menu">
            <button
              type="button"
              role="menuitem"
              className="topbar__menu-item"
              onClick={() => {
                setSettingsOpen(false);
                navigate("/subscriptions");
              }}
            >
              Subscription
            </button>
            <button
              type="button"
              role="menuitem"
              className="topbar__menu-item"
              onClick={() => {
                setSettingsOpen(false);
                navigate("/library");
              }}
            >
              Library
            </button>
          </div>
        )}
      </div>

      <div className="topbar__center">
        <button
          type="button"
          className="topbar__control topbar__control--home"
          aria-label="Home"
          onClick={() => navigate("/")}
        >
          <img
            src={homeIcon}
            alt=""
            aria-hidden="true"
            width="24"
            height="24"
          />
        </button>

        <div className="topbar__searchbox" ref={searchRef}>
          <div className="topbar__search">
            <SearchField
              value={query}
              onChange={handleQueryChange}
              onFocus={() => setSearchOpen(true)}
              onKeyDown={handleSearchKeyDown}
              expanded={searchOpen}
              controls={PANEL_ID}
            />
            <img
              className="topbar__search-icon"
              src={searchIcon}
              alt=""
              aria-hidden="true"
              width="24"
              height="24"
            />
          </div>

          {searchOpen && (
            <div
              className="search-panel"
              id={PANEL_ID}
              role="listbox"
              aria-label="Search results"
            >
              <SearchResults
                query={query}
                results={searchResults}
                status={searchStatus}
                error={searchError}
                searching={searching}
                recent={recentSearches}
                onClearRecent={onClearRecent}
                onSelect={handleSelect}
              />
            </div>
          )}
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
            <img
              src={accountIcon}
              alt=""
              aria-hidden="true"
              width="24"
              height="24"
            />
          )}
        </button>

        {menuOpen && isAuthenticated && (
          <div className="topbar__menu" role="menu">
            <p className="topbar__menu-identity">
              {username || email || "Signed in"}
            </p>
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
