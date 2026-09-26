import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAccount } from "@contexts/account.store";
import "@css/auth.css";

const ADMIN_ROLES = new Set(["Admin", "Moderator"]);

export default function RequireAdmin() {
  const { isLoading, isAuthenticated, user } = useAccount();
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

  if (!ADMIN_ROLES.has(user?.role)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}