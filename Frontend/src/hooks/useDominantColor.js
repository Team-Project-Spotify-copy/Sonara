import { useEffect, useState } from "react";
import { getDominantColor, peekDominantColor } from "@utils/dominantColor";

export default function useDominantColor(url) {
  const [color, setColor] = useState(() => peekDominantColor(url));

  useEffect(() => {
    let cancelled = false;

    if (!url) {
      setColor(null);
    } else {
      const cached = peekDominantColor(url);

      if (cached) {
        setColor(cached);
      } else {
        getDominantColor(url).then((extracted) => {
          if (!cancelled) setColor(extracted);
        });
      }
    }

    return () => {
      cancelled = true;
    };
  }, [url]);

  return color;
}
