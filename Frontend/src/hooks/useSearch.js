import { useEffect, useRef, useState } from "react";
import axios from "axios";
import { searchQuery, EMPTY_RESULTS } from "../api/search.query.js";
import useDebouncedValue from "./useDebouncedValue.js";

export default function useSearch(query, options = {}) {
  const { types, pageSize = 20, minLength = 2, delay = 300 } = options;

  const debounced = useDebouncedValue(query.trim(), delay);
  const [results, setResults] = useState(EMPTY_RESULTS);
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState(null);
  const requestId = useRef(0);

  const typesKey = types?.join(",") ?? "";

  useEffect(() => {
    if (debounced.length < minLength) {
      setResults(EMPTY_RESULTS);
      setStatus("idle");
      setError(null);
      return undefined;
    }

    const controller = new AbortController();
    const id = requestId.current + 1;
    requestId.current = id;

    setStatus("loading");
    setError(null);

    searchQuery({
      q: debounced,
      types: typesKey ? typesKey.split(",") : undefined,
      pageSize,
      signal: controller.signal,
    })
      .then((data) => {
        if (id !== requestId.current) return;
        setResults(data);
        setStatus("success");
      })
      .catch((err) => {
        if (axios.isCancel(err) || err.name === "CanceledError") return;
        if (id !== requestId.current) return;
        setError(err);
        setStatus("error");
      });

    return () => controller.abort();
  }, [debounced, typesKey, pageSize, minLength]);

  return { results, status, error };
}
