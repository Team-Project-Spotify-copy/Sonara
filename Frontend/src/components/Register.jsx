import React, { useCallback } from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import image from "../assets/images/register-bg.png";
import { useNavigate, Link } from "react-router-dom";
import { AccountContext } from "../contexts/account.context";
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

    if (!executeRecaptcha) {
      console.error("ReCAPTCHA ще не завантажилася");
      return;
    }

    const captchaToken = await executeRecaptcha("register_submit");

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

      const response = await axios.post(`${api}/auth/register`, {
        email,
        username: email,
        password,
        token,
      });

      return response.data.UserId;
    } catch (error) {
      console.error("Error during register request:", error);
    }
  }

  return (
    <div className="regsiter-page">
      <div className="regsiter-container">
        <div className="regsiter-intro">
          <div className="regsiter-avatar"></div>

          <h1 className="regsiter-title">Let's get started!</h1>

          <div className="regsiter-form-wrapper">
            <form className="regsiter-form" onSubmit={onFinish}>
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

            <p className="regsiter-divider">or</p>

            <div className="regsiter-social-group">
              <button type="button" className="btn-social">
                Google
              </button>
              <button type="button" className="btn-social">
                Facebook
              </button>
            </div>

            <p className="regsiter-footer-text">
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

      <div className="regsiter-bg-image">
        <img src={image} alt="regsiter Background" />
      </div>
    </div>
  );
}

export default Register;
