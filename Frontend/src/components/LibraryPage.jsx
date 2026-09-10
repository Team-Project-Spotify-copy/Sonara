import BasePage from "../components/layout/BasePage.jsx";
import Library from "./library/Library.jsx";

export default function PodcastPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Library />
    </BasePage>
  );
}
