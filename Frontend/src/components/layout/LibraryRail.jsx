import "../../css/LibraryRail.css";

function Icon({ d }) {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <path d={d} />
    </svg>
  );
}

const PATHS = {
  search: "M10 4a6 6 0 104.24 10.24l4.26 4.26 1.41-1.41-4.26-4.26A6 6 0 0010 4zm0 2a4 4 0 110 8 4 4 0 010-8z",
  create: "M11 5h2v6h6v2h-6v6h-2v-6H5v-2h6z",
};

const SKELETON_ROWS = Array.from({ length: 7 }, (_, i) => i);

export default function LibraryRail({
  items = [],
  loading = false,
  error = null,
  onSelect,
  onSearch,
  onCreate,
}) {
  const showSkeletons = loading && items.length === 0;

  return (
    <nav className="rail" aria-label="Library">
      <div className="rail__header">
        <h2 className="rail__title">Library</h2>
        <div className="rail__actions">
          <button
            type="button"
            className="rail__action"
            aria-label="Search your library"
            onClick={onSearch}
          >
            <Icon d={PATHS.search} />
          </button>
          <button
            type="button"
            className="rail__action"
            aria-label="Create playlist"
            onClick={onCreate}
          >
            <Icon d={PATHS.create} />
          </button>
        </div>
      </div>

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
            : items.map((item) => (
                <li key={item.id} className="rail__row">
                  <button
                    type="button"
                    className="rail__item"
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

      {!loading && !error && items.length === 0 && (
        <p className="rail__empty">Your library is empty.</p>
      )}
    </nav>
  );
}
