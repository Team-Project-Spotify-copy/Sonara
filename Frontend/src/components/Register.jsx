import React from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import { resolveCaptchaToken } from "../utils/recaptcha";
import image from "../assets/images/register-bg.png";
import { useNavigate, Link } from "react-router-dom";
import { AccountContext } from "../contexts/account.store";
import axios from "axios";
import "../css/Register.css";

function Register() {
  const { setEmail, setUserId } = React.useContext(AccountContext);
  const navigate = useNavigate();

  const { executeRecaptcha } = useGoogleReCaptcha();

  const onFinish = async (event) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const email = formData.get("email");
    const password = formData.get("password");
    const confirmPassword = formData.get("confirm-password");

    if (password !== confirmPassword) {
      console.error("Passwords do not match");
      return;
    }

    const captchaToken = await resolveCaptchaToken(executeRecaptcha, "register_submit");

    if (!captchaToken) {
      alert("Не вдалося отримати токен reCAPTCHA");
      return;
    }

    const userId = await registerRequest(email, password, captchaToken);

    if (userId) {
      setEmail(email);
      setUserId(userId);
      navigate("/");
    }
  };

async function registerRequest(email, password, token) {
  try {
    const api = import.meta.env.VITE_API;
    var username = email.split("@")[0];

    const response = await axios.post(`${api}/auth/register`, {
      email,
      username,
      password,
      token,
    });

    console.log("Register response:", response.data);

    if (response.data.AccessToken) {
      localStorage.setItem("token", response.data.accessToken);
    }

    return response.data.userId;
  } catch (error) {
    console.error("Error during register request:", error);
  }
}

  return (
    <div className="register-page">
      <div className="register-container">
        <div className="register-intro">
          <div className="register-avatar"></div>

          <h1 className="register-title">Let's get started!</h1>

          <div className="register-form-wrapper">
            <form className="register-form" onSubmit={onFinish}>
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

              <div className="form-group">
                <label htmlFor="confirm-password" className="form-label">
                  Repeat your Password
                </label>
                <input
                  type="password"
                  id="confirm-password"
                  name="confirm-password"
                  required
                  className="form-input"
                />
              </div>

              <button type="submit" className="btn-primary">
                Continue
              </button>
            </form>

            <p className="register-divider">or</p>

            <div className="register-social-group">
              <button type="button" className="btn-social">
                Google
              </button>
              <button type="button" className="btn-social">
                Facebook
              </button>
            </div>

            <p className="register-footer-text">
              Already have an account?{" "}
              <Link to="/login" className="app-link">
                Log in!
              </Link>
              <br />
              <Link
                to="/forgot-password"
                className="app-link"
                style={{ fontWeight: "normal" }}
              >
                Agree to our Terms of Service and Privacy Policy.
              </Link>
            </p>
          </div>
        </div>
      </div>

      <div className="register-bg-image">
        <img src={image} alt="register Background" />
      </div>
    </div>
  );
}

export default Register;
