import MediaCard from "./MediaCard.jsx";
import "../../css/Media.css";

const SKELETONS = Array.from({ length: 7 }, (_, i) => i);

export default function Shelf({ title, items = [], shape, loading = false, onSelect }) {
  const showSkeletons = loading && items.length === 0;

  return (
    <section className="shelf">
      <h2 className="shelf__title">{title}</h2>
      <div className="shelf__row">
        {showSkeletons
          ? SKELETONS.map((i) => <MediaCard key={i} item={null} shape={shape} />)
          : items.map((item) => (
              <MediaCard key={item.id} item={item} shape={shape} onSelect={onSelect} />
            ))}
      </div>
      {!loading && items.length === 0 && <p className="shelf__empty">Nothing here yet.</p>}
    </section>
  );
}
