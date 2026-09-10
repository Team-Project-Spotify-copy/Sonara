import BasePage from "../components/layout/BasePage.jsx";
import Podcast from "./playlist/Podcst.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Podcast />
    </BasePage>
  );
}
