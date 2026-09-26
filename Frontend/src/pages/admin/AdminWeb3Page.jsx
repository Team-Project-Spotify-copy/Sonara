import { useEffect, useState } from "react";
import * as adminService from "@api/admin.service.js";
import "@css/Admin.css";

const PLAN_TYPES = [
  { value: 0, label: "Individual" },
  { value: 1, label: "Duo" },
  { value: 2, label: "Family" },
];

export default function AdminWeb3Page() {
  const [logs, setLogs] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [selectedUserId, setSelectedUserId] = useState("");
  const [selectedPlan, setSelectedPlan] = useState(0);
  const [activating, setActivating] = useState(false);
  const [activateMessage, setActivateMessage] = useState(null);

  useEffect(() => {
    loadAll();
  }, []);

  async function loadAll() {
    try {
      setLoading(true);
      setError(null);
      const [logsList, usersList] = await Promise.all([
        adminService.getWeb3Logs(),
        adminService.getUsers(),
      ]);
      setLogs(logsList);
      setUsers(usersList);
    } catch (err) {
      console.error(err);
      setError("Failed to load Web3 logs.");
    } finally {
      setLoading(false);
    }
  }

  async function handleManualActivate(e) {
    e.preventDefault();
    if (!selectedUserId) return;

    setActivating(true);
    setActivateMessage(null);
    try {
      await adminService.manualActivateSubscription(selectedUserId, Number(selectedPlan));
      setActivateMessage({ type: "success", text: "The subscription has been activated." });
      loadAll();
    } catch (err) {
      console.error(err);
      setActivateMessage({ type: "error", text: "Failed to activate the subscription." });
    } finally {
      setActivating(false);
    }
  }

  return (
    <div className="admin-page">
      <h2 className="admin-page__title">Web3 / Crypto Subscriptions</h2>

      <section className="admin-section">
        <h3>Manual subscription activation</h3>
        <p className="admin-page__hint">
          Use this if a transaction on the blockchain has been completed, but the event listener hasn't picked it up.
        </p>
        <form className="admin-manual-activate" onSubmit={handleManualActivate}>
          <select
            className="admin-select"
            value={selectedUserId}
            onChange={(e) => setSelectedUserId(e.target.value)}
          >
            <option value="">Select user…</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.username} ({u.email})
              </option>
            ))}
          </select>

          <select
            className="admin-select"
            value={selectedPlan}
            onChange={(e) => setSelectedPlan(e.target.value)}
          >
            {PLAN_TYPES.map((p) => (
              <option key={p.value} value={p.value}>{p.label}</option>
            ))}
          </select>

          <button
            type="submit"
            className="btn-primary admin-manual-activate__btn"
            disabled={activating || !selectedUserId}
          >
            {activating ? "Activating…" : "Activate"}
          </button>
        </form>

        {activateMessage && (
          <p className={activateMessage.type === "error" ? "admin-page__error" : "admin-page__success"}>
            {activateMessage.text}
          </p>
        )}
      </section>

      {error && <p className="admin-page__error">{error}</p>}
      {loading ? (
        <p className="admin-page__loading">Loading…</p>
      ) : (
        <section className="admin-section">
          <h3>Latest Events ({logs.length})</h3>
          <table className="admin-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Block</th>
                <th>User</th>
                <th>Plan</th>
                <th>Wallet</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id}>
                  <td>{new Date(log.processedAt).toLocaleString()}</td>
                  <td>{log.blockNumber}</td>
                  <td>{log.resolvedUsername ?? log.rawUserId}</td>
                  <td>{PLAN_TYPES[log.planType]?.label ?? log.planType}</td>
                  <td className="admin-table__wallet">{log.buyer}</td>
                  <td>
                    <span className={`admin-status admin-status--${log.status.toLowerCase()}`}>
                      {log.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}
    </div>
  );
}