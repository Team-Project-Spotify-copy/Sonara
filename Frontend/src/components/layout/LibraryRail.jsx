import bookIcon from "@assets/icons/book.svg";
import plusIcon from "@assets/icons/plus.svg";
import "@css/LibraryRail.css";

const SKELETON_ROWS = Array.from({ length: 7 }, (_, i) => i);

/**
 * Figma 707:3845 (collapsed, 125px) and 707:4772 (expanded, 415px).
 *
 * Both states render the same rows; collapsing narrows the rail so the label
 * column is clipped and only the 93px artwork stays visible. The Copper button
 * is the toggle in both states — in the collapsed rail the two buttons stack
 * vertically, in the expanded rail they sit beside the "Library" heading.
 */

const FILTERS = [
  { key: "all", label: "All" },
  { key: "playlists", label: "Playlists" },
  { key: "podcasts", label: "Podcasts" },
  { key: "artists", label: "Artist" },
];

export default function LibraryRail({
  items = [],
  loading = false,
  error = null,
  onSelect,
  onCreate,
  expanded = false,
  onToggleExpanded,
  filter = "all",
  onFilterChange,
}) {
  const showSkeletons = loading && items.length === 0;

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
    <button type="button" className="rail__action" aria-label="Create playlist" onClick={onCreate}>
      <img src={plusIcon} alt="" aria-hidden="true" width="24" height="24" />
    </button>
  );

  const visibleItems =
    filter === "all"
      ? items
      : items.filter((item) => `${item.kind}s` === filter || item.kind === filter);

  return (
    <nav className={`rail${expanded ? " rail--expanded" : ""}`} aria-label="Library">
      {expanded ? (
        <div className="rail__header">
          <div className="rail__header-top">
            <h2 className="rail__title">Library</h2>
            <div className="rail__actions">
              {createButton}
              {toggleButton}
            </div>
          </div>
          <div className="rail__filters" role="tablist" aria-label="Filter library">
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
                    title={expanded ? undefined : `${item.title} — ${item.subtitle}`}
                    onClick={() => onSelect?.(item)}
                  >
                    <span
                      className={`rail__art${item.kind === "artist" ? " rail__art--round" : ""}`}
                      style={
                        item.imageUrl ? { backgroundImage: `url(${item.imageUrl})` } : undefined
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
    </nav>
  );
}
