import React, { useCallback, useState } from "react";
import AppShell from "@components/layout/AppShell.jsx";
import TopBar from "@components/layout/TopBar.jsx";
import LibraryRail from "@components/layout/LibraryRail.jsx";
import useSearch from "@hooks/useSearch.js";
import useLibrary from "@hooks/useLibrary.js";
import useRecentSearches from "@hooks/useRecentSearches.js";
import { usePlayer } from "@contexts/player.store";
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

  const {
    items: recentSearches,
    remember: rememberSearch,
    clear: clearRecentSearches,
  } = useRecentSearches();

  const { setQueueAndPlay } = usePlayer();

  const searching = query.trim().length >= MIN_QUERY_LENGTH;

  const defaultHandleSelect = useCallback(
    (item, shelfItems = []) => {
      if (item.kind === "track") {
        const tracks = shelfItems.filter((entry) => entry.kind === "track");
        const startIndex = tracks.indexOf(item);

        setQueueAndPlay(tracks.length ? tracks : [item], Math.max(0, startIndex), { autoplay: true });
        navigate(`/song/${item.id}`);
      }
      if (item.kind === "playlist") navigate(`/playlist/${item.title}`);
      if (item.kind === "podcast") navigate(`/podcast/${item.title}`);
      if (item.kind === "album") navigate(`/album/${item.title}`);
      if (item.kind === "artist") navigate(`/account/${item.title}`);
    },
    [navigate, setQueueAndPlay],
  );

  const handleSelect = customOnSelect || defaultHandleSelect;

  // Picking from the dropdown both opens the item and feeds the recent list
  // the panel shows before a query is typed (Figma 686:1212).
  const handleSearchSelect = useCallback(
    (item) => {
      rememberSearch(item);
      handleSelect(item);
    },
    [handleSelect, rememberSearch],
  );

  return (
    <AppShell
      railExpanded={railExpanded}
      topBar={
        <TopBar
          query={query}
          onQueryChange={setQuery}
          searching={searching}
          searchResults={results}
          searchStatus={searchStatus}
          searchError={searchError}
          recentSearches={recentSearches}
          onClearRecent={clearRecentSearches}
          onSearchSelect={handleSearchSelect}
        />
      }
      showBackdrop={showBackdrop}
      showMain={showMain}
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
      style={{ "--panel-padding": "0px", ...customStyle }}
    >
      {React.Children.map(children, (child) =>
        React.isValidElement(child)
          ? React.cloneElement(child, { onLibraryChange: refetchLibrary })
          : child,
      )}
    </AppShell>
  );
}
