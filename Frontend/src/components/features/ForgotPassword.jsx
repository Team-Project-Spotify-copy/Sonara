import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { forgotPassword } from "@api/auth.service.js";
import image from "@assets/images/forgot-ps-bg.png";
import logo from "@assets/icons/sonara-mark.svg";
import "@css/ResetPassword.css";
import "@css/auth.css";

function ForgotPassword() {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const onSubmit = async (event) => {
    event.preventDefault();
    if (submitting) return;

    const email = String(new FormData(event.currentTarget).get("email") ?? "").trim();

    if (!email.includes("@")) {
      setError("Enter a valid email address.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await forgotPassword(email);
      // The API answers the same way whether or not the address exists, so we
      // always move on to the code screen and never leak account existence.
      navigate("/reset-password", { state: { email } });
    } catch (err) {
      setError(err?.message ?? "Could not send the reset code. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="reset-password-page">
      <div className="reset-password-container">
        <button
          type="button"
          className="back-btn"
          onClick={() => navigate(-1)}
          aria-label="Go back"
        >
          <svg
            width="32"
            height="32"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2.5"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
          >
            <line x1="19" y1="12" x2="5" y2="12"></line>
            <polyline points="12 19 5 12 12 5"></polyline>
          </svg>
        </button>

        <div className="reset-password-intro">
          <img src={logo} alt="Sonara" className="login-logo" />

          <h1 className="reset-password-title">Forgot Your Password?</h1>
          <p className="reset-password-text-under-title">
            Enter your email and we&apos;ll send you a <br /> code to reset your
            password.
          </p>

          <div className="reset-password-form-wrapper">
            <form
              className="reset-password-form"
              onSubmit={onSubmit}
              noValidate
            >
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

              <button
                type="submit"
                className="reset-password-btn"
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
                {submitting ? "Sending…" : "Continue"}
              </button>
            </form>

            <p className="reset-password-footer-text" style={{marginTop: "20px"}}>
              Remembered it?{" "}
              <Link to="/login" className="app-link">
                Log in
              </Link>
            </p>
          </div>
        </div>
      </div>

      <div className="reset-password-bg-image">
        <img src={image} alt="" aria-hidden="true" />
      </div>
    </div>
  );
}

export default ForgotPassword;
