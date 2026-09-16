import BasePage from "@components/layout/BasePage.jsx";
import Library from "@components/library/Library.jsx";

export default function LibraryPage() {
  return (
    <BasePage showBackdrop={false} showMain={false}>
      <Library />
    </BasePage>
  );
}
