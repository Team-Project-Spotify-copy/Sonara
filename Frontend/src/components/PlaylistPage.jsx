import { useState } from "react";
import AppShell from "../components/layout/AppShell.jsx";
import TopBar from "../components/layout/TopBar.jsx";
import LibraryRail from "../components/layout/LibraryRail.jsx";
import Shelf from "../components/media/Shelf.jsx";
import SearchResults from "../components/search/SearchResults.jsx";
import Playlist from "./playlist/Playlist.jsx";
import useSearch from "../hooks/useSearch.js";
import useHomeFeed from "../hooks/useHomeFeed.js";
import useLibrary from "../hooks/useLibrary.js";
import { usePlayer } from "../contexts/player.store";

const MIN_QUERY_LENGTH = 2;

export default function PlaylistPage
() {
  const [query, setQuery] = useState("");

  const {
    results,
    status: searchStatus,
    error: searchError,
  } = useSearch(query, {
    minLength: MIN_QUERY_LENGTH,
  });
  const { shelves, status: feedStatus } = useHomeFeed();
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
      topBar={<TopBar query={query} onQueryChange={setQuery} />}
      rail={
        <LibraryRail
          items={libraryItems}
          loading={libraryStatus === "loading"}
          error={libraryError}
          onSelect={handleSelect}
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
        
          <Playlist/>
        
      )}
    </AppShell>
  );
}
