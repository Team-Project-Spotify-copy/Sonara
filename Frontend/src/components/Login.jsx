import React from "react";
import image from "../assets/images/login-bg.png";
import "../css/Login.css"; 

function Login() {
  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-intro">
          <div className="login-avatar"></div>

          <h1 className="login-title">Welcome back!</h1>

          <div className="login-form-wrapper">
            <form className="login-form">
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

              <button type="submit" className="btn-primary">
                Continue
              </button>
            </form>

            <p className="login-divider">or</p>

            <div className="login-social-group">
              <button type="button" className="btn-social">
                Google
              </button>
              <button type="button" className="btn-social">
                Facebook
              </button>
            </div>

            <p className="login-footer-text">
              Don't have an account?{" "}
              <a href="/register" className="app-link">
                Sign up!
              </a>
              <br />
              <a
                href="/forgot-password"
                className="app-link"
                style={{ fontWeight: "normal" }}
              >
                Forgot your password?
              </a>
            </p>
          </div>
        </div>
      </div>

      <div className="login-bg-image">
        <img src={image} alt="Login Background" />
      </div>
    </div>
  );
}

export default Login;
