import BasePage from "../components/layout/BasePage.jsx";
import Account from "./user/Account.jsx";
import { usePlayer } from "../contexts/player.store.js";
import { useNavigate } from "react-router-dom";

export default function AccountPage() {
  const { setQueueAndPlay } = usePlayer();
  const navigate = useNavigate();

  const handleSelect = (item, shelfItems = []) => {
    if (item.kind === "track") {
      const tracks = shelfItems
        .filter((entry) => entry.kind === "track")
        .map((entry) => entry.track || entry);
      const picked = item.track || item;
      const startIndex = tracks.indexOf(picked);

      setQueueAndPlay(tracks.length ? tracks : [picked], Math.max(0, startIndex), { autoplay: true });
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
