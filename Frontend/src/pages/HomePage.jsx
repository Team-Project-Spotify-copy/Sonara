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
    console.log(item)
    if (item.kind == "track") 
      navigate(`/song/${item.id}`);
    if (item.kind == "playlist") 
      navigate(`/playlist/${item.title}`);
    if (item.kind == "podcast") 
      navigate(`/podcast/${item.title}`);
    if (item.kind == "album") 
      navigate(`/album/${item.title}`);
    if (item.kind == "artist") 
      navigate(`/account/${item.title}`);
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
