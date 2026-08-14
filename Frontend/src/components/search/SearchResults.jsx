import MediaCard from "../media/MediaCard.jsx";
import "../../css/Media.css";
import "../../css/Search.css";

const GROUPS = [
  { key: "tracks", title: "Songs", shape: "square" },
  { key: "artists", title: "Artists", shape: "round" },
  { key: "albums", title: "Albums", shape: "square" },
  { key: "playlists", title: "Playlists", shape: "square" },
  { key: "podcasts", title: "Podcasts", shape: "square" },
];

const SKELETONS = Array.from({ length: 10 }, (_, i) => i);

export default function SearchResults({ query, results, status, error, onSelect }) {
  if (status === "error") {
    return (
      <p className="search-results__error">
        Search failed: {error?.message ?? "unknown error"}
      </p>
    );
  }

  if (status === "loading") {
    return (
      <div className="media-grid">
        {SKELETONS.map((i) => (
          <MediaCard key={i} item={null} />
        ))}
      </div>
    );
  }

  const groups = GROUPS.filter((group) => results?.[group.key]?.length > 0);

  if (groups.length === 0) {
    return <p className="search-results__status">No results for “{query}”.</p>;
  }

  return (
    <div className="search-results">
      {groups.map((group) => (
        <section key={group.key} className="shelf">
          <h2 className="shelf__title">{group.title}</h2>
          <div className="media-grid">
            {results[group.key].map((item) => (
              <MediaCard
                key={item.id}
                item={item}
                shape={group.shape}
                onSelect={onSelect}
              />
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}
