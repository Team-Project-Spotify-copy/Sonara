import BasePage from "../components/layout/BasePage.jsx";
import Shelf from "../components/media/Shelf.jsx";
import useHomeFeed from "../hooks/useHomeFeed.js";
import { usePlayer } from "../contexts/player.store.js";
import { useNavigate } from "react-router-dom";

export default function HomePage() {
  const { shelves, status: feedStatus } = useHomeFeed();
  const { setQueueAndPlay } = usePlayer();
  const navigate = useNavigate();

  const handleSelect = (item) => {
    if (item.kind === "track") {
      setQueueAndPlay([item], 0, { autoplay: true });
      navigate(`/song/${item.id}`);
    }
    if (item.kind === "playlist") navigate(`/playlist/${item.title}`);
    if (item.kind === "podcast") navigate(`/podcast/${item.title}`);
    if (item.kind === "album") navigate(`/album/${item.title}`);
    if (item.kind === "artist") navigate(`/account/${item.title}`);
  };

  return (
    <BasePage customOnSelect={handleSelect} customStyle={{"--panel-padding": "24px"}}>
      {shelves.map((shelf) => (
        <Shelf
          key={shelf.key}
          title={shelf.title}
          shape={shelf.shape}
          items={shelf.items}
          loading={feedStatus === "loading"}
          onSelect={handleSelect}
        />
      ))}
    </BasePage>
  );
}
