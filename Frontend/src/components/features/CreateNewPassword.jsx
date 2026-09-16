import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { resetPassword } from "@api/auth.service.js";
import image from "@assets/images/create-new-ps-bg.png";
import logo from "@assets/icons/sonara-mark.svg";
import "@css/CreateNewPassword.css";
import "@css/auth.css";

const MIN_PASSWORD_LENGTH = 8;

function CreateNewPassword() {
  const navigate = useNavigate();
  const location = useLocation();
  const { email, resetToken } = location.state ?? {};

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [done, setDone] = useState(false);

  // The reset token is single-use and short lived; without it this screen
  // cannot do anything, so send the user back to the start of the flow.
  if (!email || !resetToken) {
    return <Navigate to="/forgot-password" replace />;
  }

  const onSubmit = async (event) => {
    event.preventDefault();
    if (submitting) return;

    const formData = new FormData(event.currentTarget);
    const newPassword = String(formData.get("password") ?? "");
    const confirmPassword = String(formData.get("confirm-password") ?? "");

    // Mirrors ResetPasswordCommandHandler's server-side rules.
    if (newPassword.length < MIN_PASSWORD_LENGTH) {
      setError(`Password must be at least ${MIN_PASSWORD_LENGTH} characters long.`);
      return;
    }
    if (newPassword !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await resetPassword({ email, resetToken, newPassword, confirmPassword });
      setDone(true);
      // All sessions were revoked server-side, so the user signs in again.
      setTimeout(() => navigate("/login", { replace: true }), 1200);
    } catch (err) {
      setError(err?.message ?? "Could not reset your password. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="create-new-password-page">
      <div className="create-new-password-container">
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

        <div className="create-new-password-intro">
          <img src={logo} alt="Sonara" className="login-logo" />

          <h1 className="create-new-password-title">Create New Password</h1>

          <div className="create-new-password-form-wrapper">
            <form className="create-new-password-form" onSubmit={onSubmit} noValidate>
              {error && (
                <p className="auth-message auth-message--error" role="alert">
                  {error}
                </p>
              )}
              {done && (
                <p className="auth-message auth-message--success" role="status">
                  Password changed. Redirecting you to sign in…
                </p>
              )}

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
                  disabled={submitting || done}
                  className="form-input"
                  style={{ backgroundColor: "#1b1b1b", border: " 2px solid white" }}
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
                  disabled={submitting || done}
                  className="form-input"
                  style={{ backgroundColor: "#1b1b1b", border: " 2px solid white" }}
                />
              </div>

              <button
                type="submit"
                className="btn-primary"
                disabled={submitting || done}
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
                {submitting ? "Saving…" : "Continue"}
              </button>
            </form>

            <p className="create-new-password-footer-text">
              Agree to our Terms o Services and Privacy Policy.
            </p>
          </div>
        </div>
      </div>

      <div className="create-new-password-bg-image">
        <img src={image} alt="" aria-hidden="true" />
      </div>
    </div>
  );
}

export default CreateNewPassword;