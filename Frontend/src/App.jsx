import { BrowserRouter, Route, Routes, Navigate } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google";
import Home from "@pages/HomePage.jsx";
import Login from "@pages/LoginPage.jsx";
import Register from "@pages/RegisterPage.jsx";
import ForgotPassword from "@pages/ForgotPasswordPage.jsx";
import CreateNewPassword from "@pages/CreateNewPasswordPage.jsx";
import Subscription from "@pages/SubscriptionPage.jsx";
import ResetPassword from "@pages/ResetPasswordPage.jsx";
import Account from "@pages/AccountPage.jsx";
import Playlist from "@pages/PlaylistPage.jsx";
import Podcast from "@pages/PodcastPage.jsx";
import Album from "@pages/AlbumPage.jsx";
import Library from "@pages/LibraryPage.jsx";
import Song from "@pages/SongPage.jsx";
import RootLayout from "@layouts/RootLayout.jsx";
import RequireAuth from "@components/auth/RequireAuth.jsx";
import { PlayerProvider } from "@contexts/player.context.jsx";
import AdminLayout from "@layouts/AdminLayout.jsx";
import AdminCatalogPage from "@pages/admin/AdminCatalogPage.jsx";
import RequireAdmin from "@components/auth/RequireAdmin.jsx";
import AdminPodcastsPage from "@pages/admin/AdminPodcastsPage.jsx";
import AdminUsersPage from "@pages/admin/AdminUsersPage.jsx";
import AdminWeb3Page from "@pages/admin/AdminWeb3Page.jsx";
import "./index.css";

const clientId = import.meta.env.VITE_CLIENT_ID;

function App() {
  return (
    <>
      <GoogleOAuthProvider clientId={clientId}>
        <BrowserRouter>
          <PlayerProvider>
            <Routes>
              <Route element={<RootLayout />}>
                {/* Public */}
                <Route path="/" element={<Home />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/forgot-password" element={<ForgotPassword />} />
                <Route path="/reset-password" element={<ResetPassword />} />
                <Route path="/create-new-password" element={<CreateNewPassword />} />
                <Route path="/song" element={<Song />} />
                <Route path="/song/:id" element={<Song />} />
                <Route path="/playlist/:name" element={<Playlist />} />
                <Route path="/podcast/:name" element={<Podcast />} />
                <Route path="/album/:name" element={<Album />} />
              </Route>
              
              {/* Admin only */}
              <Route element={<RequireAdmin />}>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route index element={<Navigate to="catalog" replace />} />
                  <Route path="catalog" element={<AdminCatalogPage />} />
                  <Route path="podcasts" element={<AdminPodcastsPage />} />
                  <Route path="users" element={<AdminUsersPage />} />
                  <Route path="web3" element={<AdminWeb3Page />} />
                </Route>
              </Route>

              {/* Signed-in only */}
              <Route element={<RequireAuth />}>
                <Route path="/library" element={<Library />} />
                <Route path="/account" element={<Account />} />
                <Route path="/account/:username" element={<Account />} />
                <Route path="/subscriptions" element={<Subscription />} />
              </Route>
            </Routes>
          </PlayerProvider>
        </BrowserRouter>
      </GoogleOAuthProvider>
    </>
  );
}

export default App;
