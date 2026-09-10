import { BrowserRouter, Route, Routes } from "react-router-dom";
import HomePage from "./pages/HomePage.jsx";
import Login from "./components/Login.jsx";
import Register from "./components/Register.jsx";
import ForgotPassword from "./components/ForgotPassword.jsx";
import CreateNewPassword from "./components/CreateNewPassword.jsx";
import SubscriptionPage from "./components/SubscriptionPage.jsx";
import ResetPassword from "./components/ResetPassword.jsx";
import Account from "./components/AccountPage.jsx";
import PlaylistPage from "./components/PlaylistPage.jsx";
import PodcastPage from "./components/PodcastPage.jsx";
import AlbumPage from "./components/AlbumPage.jsx";
import Library from "./components/LibraryPage.jsx";
import Song from "./components/Song.jsx";
import RootLayout from "./layouts/RootLayout.jsx";
import RequireAuth from "./components/auth/RequireAuth.jsx";
import { PlayerProvider } from "./contexts/player.context.jsx";
import "./index.css";

function App() {
  return (
    <>
      <BrowserRouter>
        <PlayerProvider>
          <Routes>
            <Route element={<RootLayout />}>
              {/* Public */}
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/forgot-password" element={<ForgotPassword />} />
              <Route path="/reset-password" element={<ResetPassword />} />
              <Route
                path="/create-new-password"
                element={<CreateNewPassword />}
              />
              <Route path="/song" element={<Song />} />
              <Route path="/song/:id" element={<Song />} />
              <Route path="/playlist/:name" element={<PlaylistPage />} />
              <Route path="/podcast/:name" element={<PodcastPage />} />
              <Route path="/album/:name" element={<AlbumPage />} />

              {/* Signed-in only */}
              <Route element={<RequireAuth />}>
                <Route path="/library" element={<Library />} />
                <Route path="/account" element={<Account />} />
                <Route path="/account/:username" element={<Account />} />
                <Route path="/subscriptions" element={<SubscriptionPage />} />
              </Route>
            </Route>
          </Routes>
        </PlayerProvider>
      </BrowserRouter>
    </>
  );
}

export default App;
