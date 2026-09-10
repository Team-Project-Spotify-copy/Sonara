import React, { useState } from "react";
import AppShell from "./AppShell.jsx";
import TopBar from "./TopBar.jsx";
import LibraryRail from "./LibraryRail.jsx";
import SearchResults from "../search/SearchResults.jsx";
import useSearch from "../../hooks/useSearch.js";
import useLibrary from "../../hooks/useLibrary.js";
import { usePlayer } from "../../contexts/player.store";
import { useNavigate } from "react-router-dom";

const MIN_QUERY_LENGTH = 2;

export default function BasePage({
  children,
  showRail = true,
  initialFilter = "all",
  customOnSelect,
  showBackdrop, 
  showMain, 
  customStyle = {},
}) {
  const [query, setQuery] = useState("");
  const [railExpanded, setRailExpanded] = useState(false);
  const [libraryFilter, setLibraryFilter] = useState(initialFilter);
  const navigate = useNavigate();

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
    refetch: refetchLibrary,
  } = useLibrary();

  const { setQueueAndPlay } = usePlayer();

  const searching = query.trim().length >= MIN_QUERY_LENGTH;

const defaultHandleSelect = (item) => {
  console.log(item)

  if (item.kind === "track") {
    setQueueAndPlay([item], 0, { autoplay: true });
    navigate(`/song/${item.id}`);
  }
  if (item.kind === "playlist") navigate(`/playlist/${item.title}`);
  if (item.kind === "podcast") navigate(`/podcast/${item.title}`);
  if (item.kind === "album") navigate(`/album/${item.title}`);
  if (item.kind === "artist") navigate(`/account/${item.title}`);
};

  const handleSelect = customOnSelect || defaultHandleSelect;

  return (
    <AppShell
      railExpanded={railExpanded}
      topBar={<TopBar query={query} onQueryChange={setQuery} />}
      showBackdrop={
        showBackdrop !== undefined ? showBackdrop : searching ? true : undefined
      }
      showMain={
        showMain !== undefined ? showMain : searching ? true : undefined
      }
      showRail={showRail}
      rail={
        showRail && (
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
        )
      }
      style={{
        "--panel-padding": searching ? "24px" : "0px",
        ...customStyle,
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
        React.Children.map(children, (child) =>
          React.isValidElement(child)
            ? React.cloneElement(child, { onLibraryChange: refetchLibrary })
            : child,
        )
      )}
    </AppShell>
  );
}
