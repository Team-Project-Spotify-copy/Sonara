import { useState } from "react";
import "../../css/AddEntityModal.css";

export default function AddEntityModal({ type, onClose, onSuccess }) {
  const [formData, setFormData] = useState({
    name: "",
    title: "",
    description: "",
    isPrivate: false,
    coverImage: null,
    username: "",
  });

  const [previewUrl, setPreviewUrl] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const isArtist = type === "artist";
  const isPlaylist = type === "playlist";
  const isPodcast = type === "podcast";

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFormData({ ...formData, coverImage: file });
      setPreviewUrl(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      let endpoint = "";
      let options = {};

      const accessToken =
        localStorage.getItem("token") || localStorage.getItem("accessToken");

      const headers = {
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      };

      if (isArtist) {
        console.log("Відправляємо username:", formData.username);
        endpoint = `https://localhost:7083/api/profile/${encodeURIComponent(formData.username)}/follow`;
        options = {
          method: "POST",
          headers: headers,

        };
      } else if (isPlaylist) {
        endpoint = "https://localhost:7083/api/playlists";

        const data = new FormData();
        data.append("Name", formData.name);
        if (formData.description)
          data.append("Description", formData.description);
        data.append("IsPrivate", formData.isPrivate);
        if (formData.coverImage) data.append("CoverImage", formData.coverImage);

        options = {
          method: "POST",
          headers: headers,
          body: data,
        };
      } else if (isPodcast) {
        endpoint = "https://localhost:7083/api/podcasts";

        const data = new FormData();
        data.append("Title", formData.title);
        if (formData.description)
          data.append("Description", formData.description);
        if (formData.coverImage) data.append("CoverImage", formData.coverImage);

        options = {
          method: "POST",
          headers: headers,
          body: data,
        };
      }

      const response = await fetch(endpoint, options);

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Помилка при виконанні запиту");
      }

      const contentType = response.headers.get("content-type");
      let createdItem = null;
      if (contentType && contentType.includes("application/json")) {
        createdItem = await response.json();
      }

      onSuccess(createdItem);
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
        <h2>
          {isArtist
            ? "Follow Artist"
            : isPlaylist
              ? "Create Playlist"
              : "Add Podcast"}
        </h2>

        {error && (
          <div
            className="modal-error"
            style={{ color: "red", marginBottom: "10px" }}
          >
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          {!isArtist && (
            <div style={{ marginTop: "4px" }}>
              <div className="image-upload-container">
                <div className="image-preview">
                  {previewUrl ? (
                    <img src={previewUrl} alt="Cover Preview" />
                  ) : (
                    <span>No image</span>
                  )}
                </div>

                <label className="file-input-label">
                  Choose file
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageChange}
                  />
                </label>
              </div>
            </div>
          )}

          {isArtist ? (
            <input
              type="text"
              placeholder="Enter artist username..."
              value={formData.username}
              onChange={(e) =>
                setFormData({ ...formData, username: e.target.value })
              }
              required
            />
          ) : (
            <>
              <input
                type="text"
                placeholder={
                  isPlaylist ? "Playlist name..." : "Podcast title..."
                }
                value={isPlaylist ? formData.name : formData.title}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    [isPlaylist ? "name" : "title"]: e.target.value,
                  })
                }
                required
              />

              <textarea
                placeholder="Description (optional)..."
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
              />

              {isPlaylist && (
                <label
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "8px",
                    marginTop: "4px",
                    color: "#fff",
                  }}
                >
                  <input
                    type="checkbox"
                    checked={formData.isPrivate}
                    onChange={(e) =>
                      setFormData({ ...formData, isPrivate: e.target.checked })
                    }
                  />
                  Private Playlist
                </label>
              )}
            </>
          )}

          <div className="modal-actions" style={{ marginTop: "20px" }}>
            <button type="button" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading}>
              {loading ? "Saving..." : isArtist ? "Follow" : "Create"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
