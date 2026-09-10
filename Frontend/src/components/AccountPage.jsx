import BasePage from "../components/layout/BasePage.jsx";
import Account from "./user/Account.jsx";
import { usePlayer } from "../contexts/player.store.js";
import { useNavigate } from "react-router-dom";

export default function PodcastPage() {
  const { setQueueAndPlay } = usePlayer();
  const navigate = useNavigate();

  const handleSelect = (item) => {
    if (item.kind === "track") {
      setQueueAndPlay([item.track || item], 0, { autoplay: true });
      navigate(`/song/${item.id}`);
    }
    if (item.kind === "playlist") navigate(`/playlist/${item.title}`);
    if (item.kind === "podcast") navigate(`/podcast/${item.title}`);
    if (item.kind === "album") navigate(`/album/${item.title}`);
    if (item.kind === "artist") navigate(`/account/${item.title}`);
  };

  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Account onSelect={handleSelect} />
    </BasePage>
  );
}
