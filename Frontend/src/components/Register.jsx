import React from "react";
import image from "../assets/images/register-bg.png";
import "../css/Register.css";

function Register() {
  return (
    <div className="regsiter-page">
      <div className="regsiter-container">
        <div className="regsiter-intro">
          <div className="regsiter-avatar"></div>

          <h1 className="regsiter-title">Let's get started!</h1>

          <div className="regsiter-form-wrapper">
            <form className="regsiter-form">
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
              <a href="/login" className="app-link">
                Log in!
              </a>
              <br />
              <a
                href="/forgot-password"
                className="app-link"
                style={{ fontWeight: "normal" }}
              >
                Agree to our Terms of Service and Privacy Policy.
              </a>
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
