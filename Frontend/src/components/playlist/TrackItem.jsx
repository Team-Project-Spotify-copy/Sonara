import React from "react";

export default function TrackItem({
  countPosition,
  CoverUrl,
  Name,
  Artist,
  date,
  duration,
}) {
  const minutes = Math.floor(duration / 60);
  const seconds = Math.floor(duration % 60);
  const formattedSeconds = seconds < 10 ? `0${seconds}` : seconds;

  // Форматування дати у вигляд "1 Jan. 2026"
  const formattedDate = date
    ? new Date(date).toLocaleDateString("en-GB", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "";

  return (
    <div
      style={{
        width: "100%",
        height: "93px",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        padding: "0 16px",
        boxSizing: "border-box",
        color: "#b3b3b3",
        fontFamily: "sans-serif",
      }}
    >
      {/* Ліва частина: Номер, обкладинка, назва + виконавець */}
      <div style={{ display: "flex", alignItems: "center", gap: "16px" }}>
        <span
          style={{
            width: "18px",
            height: "32px",
            textAlign: "center",
            fontSize: "32px",
            fontFamily: "Inter",
            fontWeight: 500,
            color: "#B0B0B0",
          }}
        >
          {countPosition}
        </span>

        <img
          src={CoverUrl}
          alt={Name}
          style={{
            width: "93px",
            height: "93px",
            borderRadius: "8px",
            objectFit: "cover",
            backgroundColor: "#2a2a2a",
          }}
        />

        <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
          <span
            style={{
              color: "#ffffff",
              fontSize: "20px",
              fontWeight: "500",
              fontFamily: "Inter",
            }}
          >
            {Name}
          </span>
          <span
            style={{
              fontSize: "16px",
              fontFamily: "Inter",
              fontWeight: 400,
              color: "#B0B0B0",
            }}
          >
            {Artist}
          </span>
        </div>
      </div>

      {/* Права частина: Дата додавання та тривалість */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "60px",
          fontSize: "14px",
        }}
      >
        <span
          style={{ marginRight: "60px", fontSize: "16px", color: "#B0B0B0" }}
        >
          {formattedDate}
        </span>
        <span
          style={{
            width: "40px",
            textAlign: "right",
            marginRight: "-15px",
            fontSize: "16px",
            color: "#B0B0B0",
          }}
        >
          {minutes + ":" + formattedSeconds}
        </span>
      </div>
    </div>
  );
}
