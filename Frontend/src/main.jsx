import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import { AccountProvider } from "./contexts/account.context.jsx";
import { PlayerProvider } from "./contexts/player.context.jsx";
import { GoogleReCaptchaProvider } from "react-google-recaptcha-v3";

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <AccountProvider>
      <PlayerProvider>
        <GoogleReCaptchaProvider
          reCaptchaKey={import.meta.env.VITE_RECAPTCHA_SITE_KEY}>
          <App />
        </GoogleReCaptchaProvider>
      </PlayerProvider>
    </AccountProvider>
  </StrictMode>,
);
