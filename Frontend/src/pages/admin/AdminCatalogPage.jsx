import { useEffect, useState } from "react";
import * as adminService from "@api/admin.service.js";
import "@css/Admin.css";

export default function AdminCatalogPage() {
  const [tracks, setTracks] = useState([]);
  const [albums, setAlbums] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingTrack, setEditingTrack] = useState(null);
  const [editingAlbum, setEditingAlbum] = useState(null);

  useEffect(() => {
    loadAll();
  }, []);

  async function loadAll() {
    try {
      setLoading(true);
      setError(null);
      const [tracksPage, albumsList] = await Promise.all([
        adminService.getTracks(1, 100),
        adminService.getAlbums(),
      ]);
      setTracks(tracksPage.items ?? tracksPage);
      setAlbums(albumsList);
    } catch (err) {
      console.error(err);
      setError("Failed to load the catalog..");
    } finally {
      setLoading(false);
    }
  }

  async function handleDeleteTrack(id) {
    if (!window.confirm("Delete track?")) return;
    await adminService.deleteTrack(id);
    setTracks((prev) => prev.filter((t) => t.id !== id));
  }

  async function handleDeleteAlbum(id) {
    if (!window.confirm("Delete album? Tracks in it will remain without an album.")) return;
    await adminService.deleteAlbum(id);
    setAlbums((prev) => prev.filter((a) => a.id !== id));
  }

  return (
    <div className="admin-page">
      <h2 className="admin-page__title">Music Catalog</h2>

      {error && <p className="admin-page__error">{error}</p>}
      {loading ? (
        <p className="admin-page__loading">Loading…</p>
      ) : (
        <>
          <section className="admin-section">
            <h3>Tracks ({tracks.length})</h3>
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Artist</th>
                  <th>Album</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {tracks.map((track) => (
                  <tr key={track.id}>
                    <td>{track.title}</td>
                    <td>{track.artistName}</td>
                    <td>{track.albumTitle ?? "—"}</td>
                    <td className="admin-table__actions">
                      <button
                        type="button"
                        className="admin-btn admin-btn--edit"
                        onClick={() => setEditingTrack(track)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="admin-btn admin-btn--delete"
                        onClick={() => handleDeleteTrack(track.id)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>

          <section className="admin-section">
            <h3>Albums ({albums.length})</h3>
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Artist</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {albums.map((album) => (
                  <tr key={album.id}>
                    <td>{album.title}</td>
                    <td>{album.artistName}</td>
                    <td className="admin-table__actions">
                      <button
                        type="button"
                        className="admin-btn admin-btn--edit"
                        onClick={() => setEditingAlbum(album)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="admin-btn admin-btn--delete"
                        onClick={() => handleDeleteAlbum(album.id)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </>
      )}

      {editingTrack && (
        <TrackEditModal
          track={editingTrack}
          onClose={() => setEditingTrack(null)}
          onSaved={(updated) => {
            setTracks((prev) => prev.map((t) => (t.id === updated.id ? updated : t)));
            setEditingTrack(null);
          }}
        />
      )}

      {editingAlbum && (
        <AlbumEditModal
          album={editingAlbum}
          onClose={() => setEditingAlbum(null)}
          onSaved={(updated) => {
            setAlbums((prev) => prev.map((a) => (a.id === updated.id ? updated : a)));
            setEditingAlbum(null);
          }}
        />
      )}
    </div>
  );
}

function TrackEditModal({ track, onClose, onSaved }) {
  const [title, setTitle] = useState(track.title);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const formData = new FormData();
      formData.append("Title", title);
      const updated = await adminService.updateTrack(track.id, formData);
      onSaved(updated);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="admin-modal-backdrop" onClick={onClose}>
      <form className="admin-modal" onClick={(e) => e.stopPropagation()} onSubmit={handleSubmit}>
        <h3>Edit Track</h3>
        <div className="form-group">
          <label className="form-label">Name</label>
          <input
            className="form-input"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
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

function AlbumEditModal({ album, onClose, onSaved }) {
  const [title, setTitle] = useState(album.title);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const formData = new FormData();
      formData.append("Title", title);
      const updated = await adminService.updateAlbum(album.id, formData);
      onSaved(updated);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="admin-modal-backdrop" onClick={onClose}>
      <form className="admin-modal" onClick={(e) => e.stopPropagation()} onSubmit={handleSubmit}>
        <h3>Edit Album</h3>
        <div className="form-group">
          <label className="form-label">Name</label>
          <input
            className="form-input"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
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