import { useState } from "react";
import { useNavigate } from "react-router-dom";
import AppShell from "../components/layout/AppShell.jsx";
import TopBar from "../components/layout/TopBar.jsx";
import LibraryRail from "../components/layout/LibraryRail.jsx";
import Shelf from "../components/media/Shelf.jsx";
import SearchResults from "../components/search/SearchResults.jsx";
import useSearch from "../hooks/useSearch.js";
import useHomeFeed from "../hooks/useHomeFeed.js";
import useLibrary from "../hooks/useLibrary.js";

const MIN_QUERY_LENGTH = 2;

export default function HomePage() {
  const [query, setQuery] = useState("");
  // Figma 707:3844 (collapsed) <-> 707:4771 (expanded). Same page, one
  // user-toggled state - not two screens.
  const [railExpanded, setRailExpanded] = useState(false);
  const [libraryFilter, setLibraryFilter] = useState("all");

  const { results, status: searchStatus, error: searchError } = useSearch(query, {
    minLength: MIN_QUERY_LENGTH,
  });
  const { shelves, status: feedStatus } = useHomeFeed();
  const {
    items: libraryItems,
    status: libraryStatus,
    error: libraryError,
  } = useLibrary();
  const navigate = useNavigate();

  const searching = query.trim().length >= MIN_QUERY_LENGTH;

  // A track card opens the existing /song/:id page, which loads the real track
  // and hands it to the existing player. Other kinds have no route yet, so they
  // stay inert rather than gaining a half-built destination.
  const handleSelect = (item) => {
    if (item.kind !== "track") return;

    // Song.jsx loads the real track for :id and hands the catalog to the
    // existing player, so navigating is all this needs to do.
    navigate(`/song/${item.id}`);
  };

  return (
    <AppShell
      railExpanded={railExpanded}
      topBar={<TopBar query={query} onQueryChange={setQuery} />}
      rail={
        <LibraryRail
          items={libraryItems}
          loading={libraryStatus === "loading"}
          error={libraryError}
          onSelect={handleSelect}
          expanded={railExpanded}
          onToggleExpanded={() => setRailExpanded((open) => !open)}
          filter={libraryFilter}
          onFilterChange={setLibraryFilter}
        />
      }
    >
      {searching ? (
        <SearchResults
          query={query}
          results={results}
          status={searchStatus}
          error={searchError}
          onSelect={handleSelect}
        />
      ) : (
        shelves.map((shelf) => (
          <Shelf
            key={shelf.key}
            title={shelf.title}
            shape={shelf.shape}
            items={shelf.items}
            loading={feedStatus === "loading"}
            onSelect={handleSelect}
          />
        ))
      )}
    </AppShell>
  );
}
