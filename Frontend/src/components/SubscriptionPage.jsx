import BasePage from "../components/layout/BasePage.jsx";
import Subscription from "./subscription/Subsccription.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false} >
      <Subscription />
    </BasePage>
  );
}
