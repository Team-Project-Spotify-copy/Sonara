import { useEffect, useState, useContext } from "react";
import axios from "axios";
import { libraryQuery } from "../api/library.query.js";
import { AccountContext } from "../contexts/account.store";

export default function useLibrary() {
  const [items, setItems] = useState([]);
  const [status, setStatus] = useState("loading");
  const [error, setError] = useState(null);
  const { accessToken } = useContext(AccountContext);

  const fetchLibrary = async (signal) => {
    try {
      setStatus("loading");
      setError(null);

      const data = await libraryQuery({ signal });

      const normalizedItems = (data || []).map((item) => ({
        ...item,
        imageUrl: item.imageUrl || item.artworkUrl || item.raw?.coverUrl || item.coverUrl || null,
      }));

      setItems(normalizedItems);
      setStatus("success");
    } catch (err) {
      if (axios.isCancel(err) || err.name === "CanceledError") return;
      setError(err);
      setStatus("error");
    }
  };

  useEffect(() => {
    const controller = new AbortController();
    fetchLibrary(controller.signal);

    return () => {
      controller.abort();
    };
  }, [accessToken]);

  const refetch = () => {
    const controller = new AbortController();
    fetchLibrary(controller.signal);
  };

  return { items, status, error, refetch };
}