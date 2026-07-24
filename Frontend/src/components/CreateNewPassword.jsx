import React from "react";
import image from "../assets/images/create-new-ps-bg.png";
import "../css/CreateNewPassword.css";

function CreateNewPassword() {
  return (
    <div className="create-new-password-page">
      <div className="create-new-password-container">
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

        <div className="create-new-password-intro">
          <div className="create-new-password-avatar"></div>

          <h1 className="create-new-password-title">Create New Password</h1>

          <div className="regsiter-form-wrapper">
            <form className="regsiter-form">
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

            <p className="create-new-password-footer-text">
              Agree to our Terms o Services and Privacy Policy.
            </p>
          </div>
        </div>
      </div>

      <div className="create-new-password-bg-image">
        <img src={image} alt="create-new-password Background" />
      </div>
    </div>
  );
}

export default CreateNewPassword;
