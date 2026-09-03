import { useState } from "react";
import AppShell from "../components/layout/AppShell.jsx";
import TopBar from "../components/layout/TopBar.jsx";
import Library from "../components/library/Library.jsx";
import SearchResults from "../components/search/SearchResults.jsx";
import useSearch from "../hooks/useSearch.js";
import { usePlayer } from "../contexts/player.store";

const MIN_QUERY_LENGTH = 2;

export default function LibraryPage() {
  const [query, setQuery] = useState("");

  const {
    results,
    status: searchStatus,
    error: searchError,
  } = useSearch(query, {
    minLength: MIN_QUERY_LENGTH,
  });
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
      showBackdrop={searching ? true : false}
      showMain={searching ? true : false}
      showRail={false}
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
        <Library />
      )}
    </AppShell>
  );
}
