import "@css/Search.css";

const GROUPS = [
  { key: "tracks", title: "Songs" },
  { key: "artists", title: "Artists" },
  { key: "albums", title: "Albums" },
  { key: "playlists", title: "Playlists" },
  { key: "podcasts", title: "Podcasts" },
];

/** Figma 686:1212 shows five rows before the panel clips. */
const SKELETONS = Array.from({ length: 4 }, (_, i) => i);

function Row({ item, onSelect }) {
  const round = item.kind === "artist";
  const image = item.imageUrl || item.raw?.avatarUrl;

  return (
    <li className="search-row">
      <button
        type="button"
        className="search-row__button"
        onClick={() => onSelect?.(item)}
      >
        <span
          className={`search-row__art${round ? " search-row__art--round" : ""}`}
          style={
            image
              ? { backgroundImage: `url(${image})` }
              : undefined
          }
        />
        <span className="search-row__meta">
          <span className="search-row__title">{item.title}</span>
          {item.subtitle && (
            <span className="search-row__subtitle">{item.subtitle}</span>
          )}
        </span>
      </button>
    </li>
  );
}

/**
 * The body of the search dropdown (Figma 686:1212): a compact list of rows
 * under the field, never a page takeover. With no query it offers the recent
 * picks; with one it lists the live results grouped by type.
 */
export default function SearchResults({
  query,
  results,
  status,
  error,
  searching,
  recent = [],
  onClearRecent,
  onSelect,
}) {
  if (!searching) {
    if (recent.length === 0) {
      return (
        <p className="search-panel__status">
          Search for songs, artists, albums, playlists and podcasts.
        </p>
      );
    }

    return (
      <>
        <div className="search-panel__head">
          <p className="search-panel__label">Recent searches</p>
          <button type="button" className="search-panel__clear" onClick={onClearRecent}>
            Clear
          </button>
        </div>
        <ul className="search-list">
          {recent.map((item) => (
            <Row key={`recent-${item.id}`} item={item} onSelect={onSelect} />
          ))}
        </ul>
      </>
    );
  }

  if (status === "error") {
    return (
      <p className="search-panel__status search-panel__status--error">
        Search failed: {error?.message ?? "unknown error"}
      </p>
    );
  }

  if (status === "loading") {
    return (
      <ul className="search-list" aria-hidden="true">
        {SKELETONS.map((i) => (
          <li key={i} className="search-row search-row--skeleton">
            <span className="search-row__art" />
            <span className="search-row__meta">
              <span className="search-row__title" />
              <span className="search-row__subtitle" />
            </span>
          </li>
        ))}
      </ul>
    );
  }

  const groups = GROUPS.filter((group) => results?.[group.key]?.length > 0);

  if (groups.length === 0) {
    return <p className="search-panel__status">No results for “{query}”.</p>;
  }

  return (
    <>
      {groups.map((group) => (
        <section key={group.key} className="search-panel__group">
          <p className="search-panel__label">{group.title}</p>
          <ul className="search-list">
            {results[group.key].map((item) => (
              <Row key={item.id} item={item} onSelect={onSelect} />
            ))}
          </ul>
        </section>
      ))}
    </>
  );
}
