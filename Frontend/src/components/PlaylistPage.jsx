import BasePage from "../components/layout/BasePage.jsx";
import Playlist from "./playlist/Playlist.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Playlist />
    </BasePage>
  );
}
