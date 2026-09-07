import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAccount } from "../../contexts/account.store";
import "../../css/auth.css";

/**
 * Route guard. While the stored session is still being validated we must not
 * redirect, otherwise a page refresh would bounce an authenticated user to
 * /login before /api/profile has answered.
 */
export default function RequireAuth() {
  const { isLoading, isAuthenticated } = useAccount();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="auth-gate" role="status" aria-live="polite">
        <span className="auth-spinner" aria-hidden="true" />
        <p className="auth-gate__label">Restoring your session…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
