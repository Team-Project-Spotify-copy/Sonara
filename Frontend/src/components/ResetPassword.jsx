import React, { useState } from "react";
import OtpInput from "react-otp-input";
import image from "../assets/images/reset-ps-bg.png";
import "../css/ResetPassword.css";

function ResetPassword() {
  const [code, setCode] = useState("");

  const handleChange = (otp) => {
    setCode(otp);
    if (otp.length === 4) {
      console.log("Введений код:", otp);
    }
  };

  return (
    <div className="reset-password-page">
      <div className="reset-password-container">
        <button
          type="button"
          className="back-btn"
          onClick={() => window.history.back()}
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
            <form className="reset-password-form">
              <div className="form-group-cst">
                <OtpInput
                  value={code}
                  onChange={handleChange}
                  numInputs={4}
                  renderSeparator={<span className="otp-separator"> </span>}
                  renderInput={(props) => <input {...props} />}
                  inputStyle="otp-field"
                  shouldAutoFocus
                />
              </div>

              <button type="submit" className="reset-password-btn">
                Continue
              </button>
            </form>

            <p className="reset-password-footer-text">
              <a href="/resend-code-for-reset-password" className="app-link">
                Resend
              </a>{" "}
              if you don't get confirmation code?
            </p>
          </div>
        </div>
      </div>

      <div className="reset-password-bg-image">
        <img src={image} alt="reset-password Background" />
      </div>
    </div>
  );
}

export default ResetPassword;
