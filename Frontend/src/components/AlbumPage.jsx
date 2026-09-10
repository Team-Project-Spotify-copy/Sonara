import BasePage from "../components/layout/BasePage.jsx";
import Album from "./playlist/Album.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Album />
    </BasePage>
  );
}
