import React, { useState } from "react";
import OtpInput from "react-otp-input";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { forgotPassword, verifyResetCode } from "../api/auth.service.js";
import image from "../assets/images/reset-ps-bg.png";
import "../css/ResetPassword.css";
import "../css/auth.css";

const CODE_LENGTH = 4;

function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();
  const email = location.state?.email ?? null;

  const [code, setCode] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [resending, setResending] = useState(false);
  const [error, setError] = useState(null);
  const [notice, setNotice] = useState(null);

  // The code is issued per address, so this screen is meaningless without one.
  if (!email) {
    return <Navigate to="/forgot-password" replace />;
  }

  const submitCode = async (value) => {
    if (submitting) return;

    setSubmitting(true);
    setError(null);
    setNotice(null);

    try {
      const resetToken = await verifyResetCode({ email, code: value });
      navigate("/create-new-password", { state: { email, resetToken } });
    } catch (err) {
      setError(err?.message ?? "That code is not valid. Please try again.");
      setCode("");
    } finally {
      setSubmitting(false);
    }
  };

  const handleChange = (value) => {
    setCode(value);
    if (value.length === CODE_LENGTH) void submitCode(value);
  };

  const onSubmit = (event) => {
    event.preventDefault();
    if (code.length === CODE_LENGTH) void submitCode(code);
    else setError(`Enter the ${CODE_LENGTH}-digit code from your email.`);
  };

  const resend = async () => {
    if (resending) return;
    setResending(true);
    setError(null);
    try {
      await forgotPassword(email);
      setNotice("We sent a new code to your email.");
    } catch (err) {
      setError(err?.message ?? "Could not resend the code.");
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="reset-password-page">
      <div className="reset-password-container">
        <button type="button" className="back-btn" onClick={() => navigate(-1)} aria-label="Go back">
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
          <div className="reset-password-avatar"></div>

          <h1 className="reset-password-title">Reset Your Password</h1>
          <p className="reset-password-text-under-title">
            Enter the code we sent you by email to reset your <br /> password.
          </p>

          <div className="reset-password-form-wrapper">
            <form className="reset-password-form" onSubmit={onSubmit} noValidate>
              {error && (
                <p className="auth-message auth-message--error" role="alert">
                  {error}
                </p>
              )}
              {notice && (
                <p className="auth-message auth-message--success" role="status">
                  {notice}
                </p>
              )}

              <div className="form-group-cst">
                <OtpInput
                  value={code}
                  onChange={handleChange}
                  numInputs={CODE_LENGTH}
                  renderSeparator={<span className="otp-separator"> </span>}
                  renderInput={(props) => <input {...props} disabled={submitting} />}
                  inputStyle="otp-field"
                  shouldAutoFocus
                />
              </div>

              <button type="submit" className="reset-password-btn" disabled={submitting}>
                {submitting && <span className="auth-spinner auth-spinner--inline" aria-hidden="true" />}
                {submitting ? "Verifying…" : "Continue"}
              </button>
            </form>

            <p className="reset-password-footer-text">
              <button
                type="button"
                className="app-link auth-logout"
                onClick={resend}
                disabled={resending}
              >
                {resending ? "Resending…" : "Resend"}
              </button>{" "}
              if you don&apos;t get confirmation code?
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

export default ResetPassword;
