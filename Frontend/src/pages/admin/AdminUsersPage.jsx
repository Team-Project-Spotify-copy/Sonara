import { useEffect, useState } from "react";
import * as adminService from "@api/admin.service.js";
import "@css/Admin.css";

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [savingUserId, setSavingUserId] = useState(null);

  useEffect(() => {
    loadAll();
  }, []);

  async function loadAll() {
    try {
      setLoading(true);
      setError(null);
      const [usersList, rolesList] = await Promise.all([
        adminService.getUsers(),
        adminService.getRoles(),
      ]);
      setUsers(usersList);
      setRoles(rolesList);
    } catch (err) {
      console.error(err);
      setError("Failed to load users.");
    } finally {
      setLoading(false);
    }
  }

  async function handleSearch(e) {
    e.preventDefault();
    setLoading(true);
    try {
      const usersList = await adminService.getUsers(search);
      setUsers(usersList);
    } finally {
      setLoading(false);
    }
  }

  async function handleRoleChange(userId, roleId) {
    setSavingUserId(userId);
    try {
      const updated = await adminService.updateUserRole(userId, roleId);
      setUsers((prev) => prev.map((u) => (u.id === userId ? updated : u)));
    } catch (err) {
      console.error(err);
      alert("Failed to change role. You may not have Admin privileges.");
    } finally {
      setSavingUserId(null);
    }
  }

  return (
    <div className="admin-page">
      <h2 className="admin-page__title">Users &amp; Roles</h2>

      <form className="admin-search" onSubmit={handleSearch}>
        <input
          className="form-input admin-search__input"
          placeholder="Search by email or username…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit" className="admin-btn">Find</button>
      </form>

      {error && <p className="admin-page__error">{error}</p>}
      {loading ? (
        <p className="admin-page__loading">Loading…</p>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Username</th>
              <th>Email</th>
              <th>Subscription</th>
              <th>Role</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.username}</td>
                <td>{user.email}</td>
                <td>{user.subscriptionPlanName ?? "—"}</td>
                <td>
                  <select
                    className="admin-select"
                    value={user.roleId}
                    disabled={savingUserId === user.id}
                    onChange={(e) => handleRoleChange(user.id, e.target.value)}
                  >
                    {roles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}