import React, { useState } from "react";
import { useGoogleReCaptcha } from "react-google-recaptcha-v3";
import { useNavigate, Link } from "react-router-dom";
import { resolveCaptchaToken } from "../utils/recaptcha";
import { useAccount } from "../contexts/account.store";
import image from "../assets/images/register-bg.png";
import "../css/Register.css";
import "../css/auth.css";

const MIN_PASSWORD_LENGTH = 8;

function Register() {
  const { register } = useAccount();
  const navigate = useNavigate();
  const { executeRecaptcha } = useGoogleReCaptcha();

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const onFinish = async (event) => {
    event.preventDefault();
    if (submitting) return;

    const formData = new FormData(event.currentTarget);
    const email = String(formData.get("email") ?? "").trim();
    const password = String(formData.get("password") ?? "");
    const confirmPassword = String(formData.get("confirm-password") ?? "");

    // Mirrors the server rules in RegisterCommandHandler.Validate.
    if (!email.includes("@")) {
      setError("Enter a valid email address.");
      return;
    }
    if (password.length < MIN_PASSWORD_LENGTH) {
      setError(`Password must be at least ${MIN_PASSWORD_LENGTH} characters long.`);
      return;
    }
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const captchaToken = await resolveCaptchaToken(executeRecaptcha, "register_submit");

      if (!captchaToken) {
        setError("Could not verify that you are human. Reload the page and try again.");
        return;
      }

      await register({
        email,
        username: email.split("@")[0],
        password,
        token: captchaToken,
      });

      // Registration returns a session, so the user lands signed in.
      navigate("/", { replace: true });
    } catch (err) {
      setError(err?.message ?? "Could not create your account. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="register-page">
      <div className="register-container">
        <div className="register-intro">
          <div className="register-avatar"></div>

          <h1 className="register-title">Let's get started!</h1>

          <div className="register-form-wrapper">
            <form className="register-form" onSubmit={onFinish} noValidate>
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
                  minLength={MIN_PASSWORD_LENGTH}
                  autoComplete="new-password"
                  disabled={submitting}
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
                  minLength={MIN_PASSWORD_LENGTH}
                  autoComplete="new-password"
                  disabled={submitting}
                  className="form-input"
                />
              </div>

              <button type="submit" className="btn-primary" disabled={submitting}>
                {submitting && <span className="auth-spinner auth-spinner--inline" aria-hidden="true" />}
                {submitting ? "Creating account…" : "Continue"}
              </button>
            </form>

            <p className="register-divider">or</p>

            <div className="register-social-group">
              <button type="button" className="btn-social" disabled title="Not available yet">
                Google
              </button>
              <button type="button" className="btn-social" disabled title="Not available yet">
                Facebook
              </button>
            </div>

            <p className="register-footer-text">
              Already have an account?{" "}
              <Link to="/login" className="app-link">
                Log in!
              </Link>
            </p>
          </div>
        </div>
      </div>

      <div className="register-bg-image">
        <img src={image} alt="" aria-hidden="true" />
      </div>
    </div>
  );
}

export default Register;
