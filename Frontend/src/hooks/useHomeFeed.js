import { useEffect, useState } from "react";
import axios from "axios";
import { feedQuery, SHELF_DEFINITIONS } from "../api/feed.query.js";

const EMPTY_SHELVES = SHELF_DEFINITIONS.map((shelf) => ({ ...shelf, items: [] }));

/**
 * @returns {{shelves: Array, status: "loading"|"success"|"error", error: Error|null}}
 */
export default function useHomeFeed() {
  const [shelves, setShelves] = useState(EMPTY_SHELVES);
  const [status, setStatus] = useState("loading");
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    let active = true;

    setStatus("loading");
    setError(null);

    feedQuery({ signal: controller.signal })
      .then((data) => {
        if (!active) return;
        setShelves(data);
        setStatus("success");
      })
      .catch((err) => {
        if (axios.isCancel(err) || err.name === "CanceledError") return;
        if (!active) return;
        setError(err);
        setStatus("error");
      });

    return () => {
      active = false;
      controller.abort();
    };
  }, []);

  return { shelves, status, error };
}
