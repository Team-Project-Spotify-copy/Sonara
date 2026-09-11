import "../../css/EntityDetailViewItem.css";

export default function EntityDetailViewItem({
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

  const formattedDate = date
    ? new Date(date).toLocaleDateString("en-GB", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "";

  return (
    <div className="entity-item-container">
      <div className="entity-item-left">
        <span className="entity-item-position">{countPosition}</span>

        <img src={CoverUrl} alt={Name} className="entity-item-cover" />

        <div className="entity-item-meta">
          <span className="entity-item-title">{Name}</span>
          <span className="entity-item-artist">{Artist}</span>
        </div>
      </div>

      <div className="entity-item-right">
        <span className="entity-item-date">{formattedDate}</span>
        <span className="entity-item-duration">
          {minutes + ":" + formattedSeconds}
        </span>
      </div>
    </div>
  );
}
