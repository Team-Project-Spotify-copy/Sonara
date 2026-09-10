import React,{ useState } from "react";
import { AccountContext } from "../../contexts/account.store";
import "../../css/AddEntityModal.css";

export default function AddTrackModal({ playlistId, onClose, onSuccess }) {
  const [trackName, setTrackName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const { accessToken } = React.useContext(AccountContext);
  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {

      const headers = {
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      };

      const endpoint = `https://localhost:7083/api/playlists/${playlistId}/tracks/${trackName}`;

      const response = await fetch(endpoint, {
        method: "POST",
        headers: headers,
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Помилка при виконанні запиту");
      }

      const contentType = response.headers.get("content-type");
      let updatedPlaylist = null;
      if (contentType && contentType.includes("application/json")) {
        updatedPlaylist = await response.json();
      }

      if (onSuccess) {
        onSuccess(updatedPlaylist);
      }
      onClose();
    } catch (err) {
      console.error(err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        <h2>Add Track to Playlist</h2>

        {error && (
          <div
            className="modal-error"
            style={{ color: "red", marginBottom: "10px" }}
          >
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Enter track Name..."
            value={trackId}
            onChange={(e) => setTrackName(e.target.value)}
            required
          />

          <div className="modal-actions" style={{ marginTop: "20px" }}>
            <button type="button" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading}>
              {loading ? "Adding..." : "Add"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
