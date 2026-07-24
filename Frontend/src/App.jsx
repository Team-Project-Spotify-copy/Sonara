import { BrowserRouter, Route, Routes } from "react-router-dom";
import Home from './components/Home.jsx'
import Login from "./components/Login.jsx";
import Register from "./components/Register.jsx";
import CreateNewPassword from "./components/CreateNewPassword.jsx";
import ResetPassword from "./components/ResetPassword.jsx";
import "./index.css";
import './App.css'
function App() {

  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/create-new-password" element={<CreateNewPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App
