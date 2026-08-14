import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import { AccountProvider } from "./contexts/account.context.jsx";
import { GoogleReCaptchaProvider } from "react-google-recaptcha-v3";

// PlayerProvider навмисно НЕ тут: він живе в App, усередині BrowserRouter
// (див. App.jsx). Два провайдери означали б два <audio> і подвійне відтворення.
createRoot(document.getElementById("root")).render(
  <StrictMode>
    <AccountProvider>
      <GoogleReCaptchaProvider
        reCaptchaKey={import.meta.env.VITE_RECAPTCHA_SITE_KEY}>
        <App />
      </GoogleReCaptchaProvider>
    </AccountProvider>
  </StrictMode>,
);
