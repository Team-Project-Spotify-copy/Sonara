import BasePage from "@components/layout/BasePage.jsx";
import Subscription from "@components/subscription/Subsccription.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false} >
      <Subscription />
    </BasePage>
  );
}
