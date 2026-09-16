import BasePage from "@components/layout/BasePage.jsx";
import Song from "@components/song/Song.jsx";

export default function SongPage() {
  return (
    <BasePage showBackdrop={false} showRail={false} showMain={false}>
      <Song />
    </BasePage>
  );
}