import "../../css/Media.css";

export default function MediaCard({ item, shape = "square", onSelect }) {
  const round = shape === "round" || item?.kind === "artist";
  const className = `media-card${round ? " media-card--round" : ""}`;

  if (!item) {
    return (
      <div className={`${className} media-card--skeleton`} aria-hidden="true">
        <div className="media-card__art" />
        <div className="media-card__meta">
          <span className="media-card__title" />
          <span className="media-card__subtitle" />
        </div>
      </div>
    );
  }

  return (
    <button type="button" className={className} onClick={() => onSelect?.(item)}>
      <span
        className="media-card__art"
        style={item.imageUrl ? { backgroundImage: `url(${item.imageUrl})` } : undefined}
      />
      <span className="media-card__meta">
        <span className="media-card__title">{item.title}</span>
        <span className="media-card__subtitle">{item.subtitle}</span>
      </span>
    </button>
  );
}
