import SearchField from "../search/SearchField.jsx";
import "../../css/TopBar.css";

export default function TopBar({ query, onQueryChange, avatarUrl, onMenuClick, onProfileClick }) {
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
      <button
        type="button"
        className="topbar__avatar"
        aria-label="Account"
        style={avatarUrl ? { backgroundImage: `url(${avatarUrl})` } : undefined}
        onClick={onProfileClick}
      />
    </div>
  );
}
