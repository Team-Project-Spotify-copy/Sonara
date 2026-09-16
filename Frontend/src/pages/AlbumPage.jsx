import BasePage from "@components/layout/BasePage.jsx";
import Album from "@components/playlist/Album.jsx";

export default function AlbumPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Album />
    </BasePage>
  );
}
