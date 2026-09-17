import { useCallback, useEffect, useState } from "react";

const STORAGE_KEY = "sonara.recent-searches";
const LIMIT = 8;

/** Only the fields the dropdown row and the select handler need. */
function slim(item) {
  return {
    id: item.id,
    kind: item.kind,
    title: item.title,
    subtitle: item.subtitle ?? "",
    imageUrl: item.imageUrl ?? item.raw?.avatarUrl ?? null,
    // Picking a track queues this entry straight into the player, which reads
    // the cover off artworkUrl - without it the player renders its placeholder.
    artworkUrl: item.artworkUrl ?? item.imageUrl ?? null,
  };
}

function read() {
  try {
    const parsed = JSON.parse(window.localStorage.getItem(STORAGE_KEY) ?? "[]");
    return Array.isArray(parsed) ? parsed.filter((item) => item?.id && item?.title) : [];
  } catch {
    return [];
  }
}

/**
 * Backs the "Recent searches" list of Figma 686:1212. It records what the user
 * actually opened from the dropdown, so the panel has real content to show
 * before a query is typed.
 */
export default function useRecentSearches() {
  const [items, setItems] = useState([]);

  useEffect(() => {
    setItems(read());
  }, []);

  const remember = useCallback((item) => {
    if (!item?.id) return;

    setItems((current) => {
      const next = [slim(item), ...current.filter((entry) => entry.id !== item.id)].slice(
        0,
        LIMIT,
      );

      try {
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      } catch {
        // Private mode or a full quota: the list stays in memory for the session.
      }

      return next;
    });
  }, []);

  const clear = useCallback(() => {
    setItems([]);
    try {
      window.localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Nothing to clean up when storage is unavailable.
    }
  }, []);

  return { items, remember, clear };
}
