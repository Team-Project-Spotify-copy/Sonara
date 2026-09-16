import BasePage from "@components/layout/BasePage.jsx";
import Playlist from "@components/playlist/Playlist.jsx";

export default function PlaylistPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Playlist />
    </BasePage>
  );
}
