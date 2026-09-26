import { Link, NavLink, Outlet } from "react-router-dom";
import "@css/Admin.css";

const NAV_ITEMS = [
  { to: "/admin/catalog", label: "Music Catalog" },
  { to: "/admin/podcasts", label: "Podcasts" },
  { to: "/admin/users", label: "Users & Roles" },
  { to: "/admin/web3", label: "Web3 Logs" },
];

export default function AdminLayout() {
  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <h1 className="admin-sidebar__title">Admin</h1>

        <Link to="/" className="admin-sidebar__back">
          ← Home page
        </Link>

        <nav className="admin-sidebar__nav">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `admin-sidebar__link${isActive ? " admin-sidebar__link--active" : ""}`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <main className="admin-shell__content">
        <Outlet />
      </main>
    </div>
  );
}