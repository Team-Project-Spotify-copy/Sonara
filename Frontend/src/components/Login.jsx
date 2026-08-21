import React from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import { resolveCaptchaToken } from "../utils/recaptcha";
import image from "../assets/images/login-bg.png";
import { useNavigate, Link } from "react-router-dom";
import { AccountContext } from "../contexts/account.store";
import axios from "axios";
import "../css/Login.css";

function Login() {
  const { setEmail, setAccessToken } = React.useContext(AccountContext);
  const navigate = useNavigate();

  const { executeRecaptcha } = useGoogleReCaptcha();

  const onFinish = async (event) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const email = formData.get("email");
    const password = formData.get("password");

    const captchaToken = await resolveCaptchaToken(executeRecaptcha, "login_submit");

    if (!captchaToken) {
      alert("Не вдалося отримати токен reCAPTCHA");
      return;
    }

    const accessToken = await loginRequest(email, password, captchaToken);

    if (accessToken) {
      setEmail(email);
      setAccessToken(accessToken);
      navigate("/");
    }
  };

  async function loginRequest(email, password, token) {
    try {
      const api = import.meta.env.VITE_API;

      const response = await axios.post(`${api}/auth/login`, {
        email,
        password,
        token,
      });

      return response.data.accessToken;
    } catch (error) {
      if (error.response) {
        console.error("Помилка від сервера:", error.response.data);
      } else if (error.request) {
        console.error("Немає відповіді від сервера:", error.request);
      } else {
        console.error("Помилка налаштування запиту:", error.message);
      }

      return null;
    }
  }

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-intro">
          <div className="login-avatar"></div>

          <h1 className="login-title">Welcome back!</h1>

          <div className="login-form-wrapper">
            <form className="login-form" onSubmit={onFinish}>
              <div className="form-group">
                <label htmlFor="email" className="form-label">
                  Email
                </label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  required
                  className="form-input"
                />
              </div>

              <div className="form-group">
                <label htmlFor="password" className="form-label">
                  Password
                </label>
                <input
                  type="password"
                  id="password"
                  name="password"
                  required
                  className="form-input"
                />
              </div>

              <button type="submit" className="btn-primary">
                Continue
              </button>
            </form>

            <p className="login-divider">or</p>

            <div className="login-social-group">
              <button type="button" className="btn-social">
                Google
              </button>
              <button type="button" className="btn-social">
                Facebook
              </button>
            </div>

            <p className="login-footer-text">
              Don't have an account?{" "}
              <Link to="/register" className="app-link">
                Sign up!
              </Link>
              <br />
              <Link
                to="/forgot-password"
                className="app-link"
                style={{ fontWeight: "normal" }}
              >
                Forgot your password?
              </Link>
            </p>
          </div>
        </div>
      </div>

      <div className="login-bg-image">
        <img src={image} alt="Login Background" />
      </div>
    </div>
  );
}

export default Login;
