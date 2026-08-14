import { useState } from "react";
import AppShell from "../components/layout/AppShell.jsx";
import TopBar from "../components/layout/TopBar.jsx";
import LibraryRail from "../components/layout/LibraryRail.jsx";
import Shelf from "../components/media/Shelf.jsx";
import SearchResults from "../components/search/SearchResults.jsx";
import PlayerBar from "../components/player/PlayerBar.jsx";
import useSearch from "../hooks/useSearch.js";
import useHomeFeed from "../hooks/useHomeFeed.js";
import useLibrary from "../hooks/useLibrary.js";
import { usePlayer } from "../contexts/player.context.jsx";

const MIN_QUERY_LENGTH = 2;

export default function HomePage() {
  const [query, setQuery] = useState("");

  const { results, status: searchStatus, error: searchError } = useSearch(query, {
    minLength: MIN_QUERY_LENGTH,
  });
  const { shelves, status: feedStatus } = useHomeFeed();
  const {
    items: libraryItems,
    status: libraryStatus,
    error: libraryError,
  } = useLibrary();
  const { play, hasActiveTrack } = usePlayer();

  const searching = query.trim().length >= MIN_QUERY_LENGTH;

  const handleSelect = (item) => {
    if (item.kind === "track") {
      play([item], 0);
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
      player={hasActiveTrack ? <PlayerBar /> : null}
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
