import React from "react";

const PATHS = {
  play: "M4 2v20l17-10z",
  pause: "M6 3h4v18H6zm8 0h4v18h-4z",
  prev: "M6 4h3v16H6zm14 0v16L9 12z",
  next: "M15 4h3v16h-3zM4 4l11 8-11 8z",
  heart: "M12 21S3 14.6 3 8.9A5 5 0 0 1 12 6a5 5 0 0 1 9 2.9C21 14.6 12 21 12 21z",
  shuffle:
    "M17 3l4 4-4 4V8h-2.6l-2.2 3-1.7-2.5L12.6 5H17zM3 8h4.2l1.5 2.2L7 12.7 5.8 11H3zM17 13l4 4-4 4v-3h-4.4l-1.8-2.6 1.7-2.5 1.9 2.6H17z",
  repeat:
    "M6 5h11l-2.5-2.5L16 1l5 5-5 5-1.5-1.5L17 7H8v3L3 6.5 8 3zM18 19H7l2.5 2.5L8 23l-5-5 5-5 1.5 1.5L7 17h9v-3l5 3.5-5 3.5z",
  volume: "M4 9h3l5-4v14l-5-4H4zm11.5-.5a5 5 0 0 1 0 7l-1.4-1.4a3 3 0 0 0 0-4.2z",
  muted:
    "M4 9h3l5-4v14l-5-4H4zm16.3-1.3l-1.4-1.4L16 9.2l-2.9-2.9-1.4 1.4L14.6 10.6l-2.9 2.9 1.4 1.4 2.9-2.9 2.9 2.9 1.4-1.4-2.9-2.9z",
  queue:
    "M3 5h12v2H3zm0 4h12v2H3zm0 4h8v2H3zm12.5-1.5V19a2.5 2.5 0 1 1-2-2.45V9l6-1.5v7.9a2.5 2.5 0 1 1-2-2.44V10z",
  expand: "M4 4h7v2H6v5H4zm9 0h7v7h-2V6h-5zM4 13h2v5h5v2H4zm14 0h2v7h-7v-2h5z",
  collapse: "M9 4h2v7H4V9h5zm4 0h2v5h5v2h-7zm0 9h7v2h-5v5h-2zM4 13h7v7H9v-5H4z",
  lyrics: "M4 4h16v2H4zm0 5h16v2H4zm0 5h11v2H4zm0 5h8v2H4z",
  close:
    "M18.3 5.7l-1.4-1.4L12 9.2 7.1 4.3 5.7 5.7l4.9 4.9-4.9 4.9 1.4 1.4 4.9-4.9 4.9 4.9 1.4-1.4-4.9-4.9z",
  drag: "M9 4h2v2H9zm4 0h2v2h-2zM9 9h2v2H9zm4 0h2v2h-2zM9 14h2v2H9zm4 0h2v2h-2zM9 19h2v2H9zm4 0h2v2h-2z",
};

export default function Glyph({ name }) {
  const path = PATHS[name];
  if (!path) return null;

  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d={path} />
    </svg>
  );
}
