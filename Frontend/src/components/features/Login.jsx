import { useState } from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { resolveCaptchaToken } from "@utils/recaptcha";
import { useAccount } from "@contexts/account.store";
import image from "@assets/images/login-bg.png";
import logo from "@assets/icons/sonara-mark.svg";
import "@css/Login.css";
import "@css/auth.css";

function Login() {
  const { login } = useAccount();
  const navigate = useNavigate();
  const location = useLocation();
  const { executeRecaptcha } = useGoogleReCaptcha();

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  // Send the user back where the guard interrupted them, if anywhere.
  const redirectTo = location.state?.from?.pathname ?? "/";

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
                  style={{ backgroundColor: "#1b1b1b", border: " 2px solid white" }}
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
                  style={{ backgroundColor: "#1b1b1b", border: " 2px solid white" }}
                />
              </div>

              <button
                type="submit"
                className="btn-primary"
                disabled={submitting}
                style={{ backgroundColor: "white", color: "black",  fontSize: "18px", fontWeight: 500, }}
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
