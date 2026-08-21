import { BrowserRouter, Route, Routes } from "react-router-dom";
import HomePage from "./pages/HomePage.jsx";
import Login from "./components/Login.jsx";
import Register from "./components/Register.jsx";
import CreateNewPassword from "./components/CreateNewPassword.jsx";
import ResetPassword from "./components/ResetPassword.jsx";
import Song from "./components/Song.jsx";
import RootLayout from "./layouts/RootLayout.jsx";
import { PlayerProvider } from "./contexts/player.context.jsx";
import "./index.css";

function App() {

  return (
    <>
      <BrowserRouter>
        <PlayerProvider>
          <Routes>
            <Route element={<RootLayout />}>
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/create-new-password" element={<CreateNewPassword />} />
              <Route path="/reset-password" element={<ResetPassword />} />
              <Route path="/song" element={<Song />} />
              <Route path="/song/:id" element={<Song />} />
            </Route>
          </Routes>
        </PlayerProvider>
      </BrowserRouter>
    </>
  );
}

export default App
