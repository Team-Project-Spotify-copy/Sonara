import { useState } from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { resolveCaptchaToken } from "@utils/recaptcha";
import { useAccount } from "@contexts/account.store";
import { useGoogleLogin } from "@react-oauth/google";
import axios from "axios";
import image from "@assets/images/login-bg.png";
import logo from "@assets/icons/sonara-mark.svg"
import "@css/Login.css";
import "@css/auth.css";

function Login() {
  const { login, applyCredentials } = useAccount();
  const navigate = useNavigate();
  const location = useLocation();
  const { executeRecaptcha } = useGoogleReCaptcha();

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const api = import.meta.env.VITE_API;
  const redirectTo = location.state?.from?.pathname ?? "/";

  const googleLogin = useGoogleLogin({
    onSuccess: async (tokenResponse) => {
      try {
        const response = await axios.post(`${api}/auth/google`, {
          accessToken: tokenResponse.access_token,
        });

        if (response.status === 200) {
         console.log(response.data)
         applyCredentials(response.data);
         navigate(redirectTo, { replace: true });
        }
      } catch (error) {
        console.error("Помилка авторизації на бекенді:", error);
      }
    },
    onError: (error) => console.log("Login Failed:", error),
  });

  const onFinish = async (event) => {
    event.preventDefault();
    if (submitting) return;

    const formData = new FormData(event.currentTarget);
    const email = String(formData.get("email") ?? "").trim();
    const password = String(formData.get("password") ?? "");

    if (!email || !password) {
      setError("Enter your email and password.");
      return;
    }

    setSubmitting(true);
    setError(null);
    
    try {
      const captchaToken = await resolveCaptchaToken(executeRecaptcha, "login_submit");

      if (!captchaToken) {
        setError("Could not verify that you are human. Reload the page and try again.");
        return;
      }

      await login({ email, password, token: captchaToken });
      navigate(redirectTo, { replace: true });
    } catch (err) {
      setError(err?.message ?? "Could not sign you in. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-intro">
          <img src={logo} alt="Sonara" className="login-logo" />

          <h1 className="login-title">Welcome back!</h1>

          <div className="login-form-wrapper">
            <form className="login-form" onSubmit={onFinish} noValidate>
              {error && (
                <p className="auth-message auth-message--error" role="alert">
                  {error}
                </p>
              )}

              <div className="form-group">
                <label htmlFor="email" className="form-label">
                  Email
                </label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  required
                  autoComplete="email"
                  disabled={submitting}
                  className="form-input"
                  style={{
                    backgroundColor: "#1b1b1b",
                    border: " 2px solid white",
                  }}
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
                  autoComplete="current-password"
                  disabled={submitting}
                  className="form-input"
                  style={{
                    backgroundColor: "#1b1b1b",
                    border: " 2px solid white",
                  }}
                />
              </div>

              <button
                type="submit"
                className="btn-primary"
                disabled={submitting}
                style={{
                  backgroundColor: "white",
                  color: "black",
                  fontSize: "18px",
                  fontWeight: 500,
                }}
              >
                {submitting && (
                  <span
                    className="auth-spinner auth-spinner--inline"
                    aria-hidden="true"
                  />
                )}
                {submitting ? "Signing in…" : "Continue"}
              </button>
            </form>

            <br />

            <button
              type="submit"
              className="btn-primary"
              disabled={submitting}
              style={{
                backgroundColor: "transparent",
                color: "white",
                fontSize: "18px",
                border: "2px solid white",
                fontWeight: 500,
                textAlign: "center",
                alignContent: "center",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "10px",
              }}
              onClick={() => googleLogin()}
            >
              <svg
                width="22"
                height="22"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                  fill="#FFFFFF"
                />
                <path
                  d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  fill="#FFFFFF"
                />
                <path
                  d="M5.84 14.1c-.22-.66-.35-1.36-.35-2.1s.13-1.44.35-2.1V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.62z"
                  fill="#FFFFFF"
                />
                <path
                  d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"
                  fill="#FFFFFF"
                />
              </svg>
              Countinue on Google
            </button>

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
        <img src={image} alt="" aria-hidden="true" />
      </div>
    </div>
  );
}

export default Login;
