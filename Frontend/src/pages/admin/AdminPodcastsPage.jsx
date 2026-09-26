import { useEffect, useState } from "react";
import * as adminService from "@api/admin.service.js";
import "@css/Admin.css";

export default function AdminPodcastsPage() {
  const [podcasts, setPodcasts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editing, setEditing] = useState(null);

  useEffect(() => {
    loadPodcasts();
  }, []);

  async function loadPodcasts() {
    try {
      setLoading(true);
      setError(null);
      const data = await adminService.getPodcasts();
      setPodcasts(data);
    } catch (err) {
      console.error(err);
      setError("Failed to load podcasts.");
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete(id) {
    if (!window.confirm("Delete podcast along with all episodes?")) return;
    await adminService.deletePodcast(id);
    setPodcasts((prev) => prev.filter((p) => p.id !== id));
  }

  return (
    <div className="admin-page">
      <h2 className="admin-page__title">Podcasts</h2>

      {error && <p className="admin-page__error">{error}</p>}
      {loading ? (
        <p className="admin-page__loading">Loading…</p>
      ) : (
        <section className="admin-section">
          <h3>Podcasts ({podcasts.length})</h3>
          <table className="admin-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Author</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {podcasts.map((podcast) => (
                <tr key={podcast.id}>
                  <td>{podcast.title}</td>
                  <td>{podcast.authorName}</td>
                  <td className="admin-table__actions">
                    <button
                      type="button"
                      className="admin-btn admin-btn--edit"
                      onClick={() => setEditing(podcast)}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="admin-btn admin-btn--delete"
                      onClick={() => handleDelete(podcast.id)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}

      {editing && (
        <PodcastEditModal
          podcast={editing}
          onClose={() => setEditing(null)}
          onSaved={(updated) => {
            setPodcasts((prev) => prev.map((p) => (p.id === updated.id ? updated : p)));
            setEditing(null);
          }}
        />
      )}
    </div>
  );
}

function PodcastEditModal({ podcast, onClose, onSaved }) {
  const [title, setTitle] = useState(podcast.title);
  const [description, setDescription] = useState(podcast.description ?? "");
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const formData = new FormData();
      formData.append("Title", title);
      formData.append("Description", description);
      const updated = await adminService.updatePodcast(podcast.id, formData);
      onSaved(updated);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="admin-modal-backdrop" onClick={onClose}>
      <form className="admin-modal" onClick={(e) => e.stopPropagation()} onSubmit={handleSubmit}>
        <h3>Edit Podcast</h3>
        <div className="form-group">
          <label className="form-label">Name</label>
          <input
            className="form-input"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
        </div>
        <div className="form-group">
          <label className="form-label">Description</label>
          <input
            className="form-input"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        <div className="admin-modal__actions">
          <button type="button" className="admin-btn" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving ? "Saving…" : "Save"}
          </button>
        </div>
      </form>
    </div>
  );
}