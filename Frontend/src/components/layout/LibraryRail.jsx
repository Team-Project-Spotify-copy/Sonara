import { useState } from "react";
import bookIcon from "@assets/icons/book.svg";
import plusIcon from "@assets/icons/plus.svg";
import AddEntityModal from "@components/library/AddEntityModal.jsx";
import "@css/LibraryRail.css";

const SKELETON_ROWS = Array.from({ length: 7 }, (_, i) => i);

const FILTERS = [
  { key: "all", label: "All" },
  { key: "playlists", label: "Playlists" },
  { key: "podcasts", label: "Podcasts" },
  { key: "artists", label: "Artist" },
];

const ADD_OPTIONS = [
  { key: "podcast", label: "New Podcast" },
  { key: "playlist", label: "New Playlist" },
  { key: "album", label: "New Album" },
  { key: "artist", label: "Follow Artist" },
];

export default function LibraryRail({
  items = [],
  loading = false,
  error = null,
  onSelect,
  expanded = false,
  onToggleExpanded,
  filter = "all",
  onFilterChange,
}) {
  const [isAddMenuOpen, setIsAddMenuOpen] = useState(false);
  const [activeModal, setActiveModal] = useState(null);

  const showSkeletons = loading && items.length === 0;

  const handleAddOptionSelect = (type) => {
    setIsAddMenuOpen(false);
    setActiveModal(type);
  };

  const toggleButton = (
    <button
      type="button"
      className="rail__action rail__action--accent"
      aria-label={expanded ? "Collapse library" : "Expand library"}
      aria-expanded={expanded}
      onClick={onToggleExpanded}
    >
      <img src={bookIcon} alt="" aria-hidden="true" width="24" height="24" />
    </button>
  );

  const createButton = (
    <div className="rail__create-wrapper">
      <button
        type="button"
        className="rail__action"
        aria-label="Create"
        aria-expanded={isAddMenuOpen}
        onClick={() => setIsAddMenuOpen((prev) => !prev)}
      >
        <img src={plusIcon} alt="" aria-hidden="true" width="24" height="24" />
      </button>

      {isAddMenuOpen && (
        <div className="rail__create-menu">
          {ADD_OPTIONS.map((opt) => (
            <button
              key={opt.key}
              type="button"
              onClick={() => handleAddOptionSelect(opt.key)}
            >
              {opt.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );

  const visibleItems =
    filter === "all"
      ? items
      : items.filter(
          (item) => `${item.kind}s` === filter || item.kind === filter,
        );

  return (
    <nav
      className={`rail${expanded ? " rail--expanded" : ""}`}
      aria-label="Library"
    >
      {expanded ? (
        <div className="rail__header">
          <div className="rail__header-top">
            <h2 className="rail__title">Library</h2>
            <div className="rail__actions">
              {createButton}
              {toggleButton}
            </div>
          </div>
          <div
            className="rail__filters"
            role="tablist"
            aria-label="Filter library"
          >
            {FILTERS.map((f) => (
              <button
                key={f.key}
                type="button"
                role="tab"
                aria-selected={filter === f.key}
                className={`rail__chip${filter === f.key ? " rail__chip--active" : ""}`}
                onClick={() => onFilterChange?.(f.key)}
              >
                {f.label}
              </button>
            ))}
          </div>
        </div>
      ) : (
        <div className="rail__actions rail__actions--stacked">
          {toggleButton}
          {createButton}
        </div>
      )}

      {error ? (
        <p className="rail__empty">Couldn’t load your library.</p>
      ) : (
        <ul className="rail__list">
          {showSkeletons
            ? SKELETON_ROWS.map((i) => (
                <li key={i} className="rail__row" aria-hidden="true">
                  <div className="rail__item rail__item--skeleton">
                    <span className="rail__art" />
                    <span className="rail__meta">
                      <span className="rail__name" />
                      <span className="rail__sub" />
                    </span>
                  </div>
                </li>
              ))
            : visibleItems.map((item) => (
                <li key={item.id} className="rail__row">
                  <button
                    type="button"
                    className="rail__item"
                    title={
                      expanded ? undefined : `${item.title} — ${item.subtitle}`
                    }
                    onClick={() => onSelect?.(item)}
                  >
                    <span
                      className={`rail__art${item.kind === "artist" ? " rail__art--round" : ""}`}
                      style={
                        item.imageUrl
                          ? { backgroundImage: `url(${item.imageUrl})` }
                          : undefined
                      }
                    />
                    <span className="rail__meta">
                      <span className="rail__name">{item.title}</span>
                      <span className="rail__sub">{item.subtitle}</span>
                    </span>
                  </button>
                </li>
              ))}
        </ul>
      )}

      {!loading && !error && visibleItems.length === 0 && (
        <p className="rail__empty">Your library is empty.</p>
      )}

      {activeModal && (
        <AddEntityModal
          type={activeModal}
          onClose={() => setActiveModal(null)}
          onSuccess={(newItem) => {
            console.log("Успішно створено/додано:", newItem);
            setActiveModal(null);
            window.location.reload();
          }}
        />
      )}
    </nav>
  );
}
