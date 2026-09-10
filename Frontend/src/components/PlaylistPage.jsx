import { useState } from "react";
import AppShell from "../components/layout/AppShell.jsx";
import TopBar from "../components/layout/TopBar.jsx";
import LibraryRail from "../components/layout/LibraryRail.jsx";
import SearchResults from "../components/search/SearchResults.jsx";
import Playlist from "./playlist/Playlist.jsx";
import useSearch from "../hooks/useSearch.js";
import useLibrary from "../hooks/useLibrary.js";
import { usePlayer } from "../contexts/player.store";

const MIN_QUERY_LENGTH = 2;

export default function PlaylistPage() {
  const [query, setQuery] = useState("");
  const [railExpanded, setRailExpanded] = useState(false);
  const [libraryFilter, setLibraryFilter] = useState("all");
  const {
    results,
    status: searchStatus,
    error: searchError,
  } = useSearch(query, {
    minLength: MIN_QUERY_LENGTH,
  });
  const {
    items: libraryItems,
    status: libraryStatus,
    error: libraryError,
  } = useLibrary();
  const { setQueueAndPlay } = usePlayer();

  const searching = query.trim().length >= MIN_QUERY_LENGTH;

  const handleSelect = (item) => {
    if (item.kind === "track") {
      setQueueAndPlay([item], 0, { autoplay: true });
    }
  };

  return (
    <AppShell
      railExpanded={railExpanded}
      topBar={<TopBar query={query} onQueryChange={setQuery} />}
      showBackdrop={searching ? true : false}
      showMain={searching ? true : false}
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
      style={{
        "--panel-padding": searching ? "24px" : "0px",
      }}
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
        <Playlist />
      )}
    </AppShell>
  );
}
