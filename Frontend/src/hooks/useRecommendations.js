import { useContext, useEffect, useState } from "react";
import axios from "axios";
import { recommendationsQuery } from "@api/recommendations.query.js";
import { AccountContext } from "@contexts/account.store";

export default function useRecommendations({ count, enabled = true } = {}) {
  const { accessToken } = useContext(AccountContext);
  const [items, setItems] = useState([]);
  const [topGenres, setTopGenres] = useState([]);
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!enabled || !accessToken) {
      setItems([]);
      setTopGenres([]);
      setStatus("idle");
      return undefined;
    }

    const controller = new AbortController();
    let active = true;

    setStatus("loading");
    setError(null);

    recommendationsQuery({ count, signal: controller.signal })
      .then((data) => {
        if (!active) return;
        setItems(data.items);
        setTopGenres(data.topGenres);
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
  }, [accessToken, count, enabled]);

  return { items, topGenres, status, error };
}
