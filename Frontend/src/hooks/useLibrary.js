import { useEffect, useState } from "react";
import axios from "axios";
import { libraryQuery } from "../api/library.query.js";

/**
 * @returns {{items: object[], status: "loading"|"success"|"error", error: Error|null}}
 */
export default function useLibrary() {
  const [items, setItems] = useState([]);
  const [status, setStatus] = useState("loading");
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    let active = true;

    setStatus("loading");
    setError(null);

    libraryQuery({ signal: controller.signal })
      .then((data) => {
        if (!active) return;
        setItems(data);
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

  return { items, status, error };
}
