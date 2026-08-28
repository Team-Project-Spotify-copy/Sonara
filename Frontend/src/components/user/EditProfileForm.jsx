import React, { useState } from "react";
import axios from "axios";
import styles from "../../css/EditProfile.module.css"; 

export default function EditProfileForm({
  profile,
  accessToken,
  onClose,
  onUpdateSuccess,
}) {
  const [username, setUsername] = useState(profile?.username || "");
  const [email, setEmail] = useState(profile?.email || "");
  const [avatarFile, setAvatarFile] = useState(null);
  const [avatarPreview, setAvatarPreview] = useState(
    profile?.avatarUrl || null,
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const api = import.meta.env.VITE_API;

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setAvatarFile(file);
      setAvatarPreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("Username", username);
      formData.append("Email", email);
      if (avatarFile) {
        formData.append("AvatarFile", avatarFile);
      }

      const response = await axios.put(`${api}/profile/update`, formData, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.status === 200) {
        onUpdateSuccess(response.data);
        onClose();
      }
    } catch (err) {
      console.error("Error updating profile:", err);
      setError(err.response?.data?.message || "Failed to update profile.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.editProfileOverlay} onClick={onClose}>
      <div
        className={styles.editProfileModal}
        onClick={(e) => e.stopPropagation()}
      >
        <h2>Edit profile</h2>

        {error && <p className={styles.errorMessage}>{error}</p>}

        <form onSubmit={handleSubmit}>
          <div className={styles.avatarUploadSection}>
            <img
              src={avatarPreview || "../../assets/images/profile.png"}
              alt="Avatar Preview"
              className={styles.avatarPreview}
            />
            <label className={styles.uploadBtn}>
              Choose avatar
              <input
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                hidden
              />
            </label>
          </div>

          <div className={styles.formGroup}>
            <label>Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className={styles.modalActions}>
            <button
              type="button"
              onClick={onClose}
              className={styles.cancelBtn}
            >
              Cancel
            </button>
            <button type="submit" disabled={loading} className={styles.saveBtn}>
              {loading ? "Saving..." : "Save"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
