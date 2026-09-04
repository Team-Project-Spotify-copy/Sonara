import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import image from "../../assets/images/profile-bg.png";
import { AccountContext } from "../../contexts/account.store";
import Shelf from "../media/Shelf";
import EditProfileForm from "./EditProfileForm";
import axios from "axios";
import "../../css/Account.css";

export default function Account() {
  const { accessToken } = React.useContext(AccountContext);
  const [profile, setProfile] = useState({});
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const { username } = useParams();

  const api = import.meta.env.VITE_API;
  const isOwnProfile = !username;

  useEffect(() => {
    GetAccountUser();
  }, [username, accessToken]);

  async function GetAccountUser() {
    try {
      setLoading(true);
      const endpoint = username
        ? `${api}/profile/${username}`
        : `${api}/profile`;

      const headers = accessToken
        ? { Authorization: `Bearer ${accessToken}` }
        : {};

      const response = await axios.get(endpoint, { headers });

      if (response.status === 200 && response.data) {
        setProfile(response.data);
        console.log(response.data);
      } else {
        console.error("Error fetching user data");
      }
    } catch (error) {
      console.error("Error fetching user data:", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleToggleFollow() {
    if (!accessToken || !username) return;

    try {
      if (profile?.isFollowing) {
        await axios.delete(`${api}/profile/${username}/unfollow`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });

        setProfile((prev) => ({
          ...prev,
          isFollowing: false,
          CountFollowers: Math.max(0, (prev.CountFollowers || 0) - 1),
        }));
      } else {
        await axios.post(
          `${api}/profile/${username}/follow`,
          {},
          {
            headers: { Authorization: `Bearer ${accessToken}` },
          },
        );

        setProfile((prev) => ({
          ...prev,
          isFollowing: true,
          CountFollowers: (prev.CountFollowers || 0) + 1,
        }));
      }
    } catch (error) {
      console.error("Error toggling follow state:", error);
      GetAccountUser();
    }
  }

  const historyItems =
    profile?.history?.map((item) => ({
      id: item.id || item.track?.id,
      title: item.track?.title || "Unknown Track",
      name: item.track?.title || "Unknown Track",
      imageUrl: item.track?.artworkUrl,
      coverUrl: item.track?.artworkUrl,
      listenedAt: item.listenedAt,
    })) || [];

  const playlistItems =
    profile?.playlists?.map((playlist) => ({
      id: playlist.id,
      title: playlist.name,
      name: playlist.name,
      imageUrl: playlist.coverUrl,
      coverUrl: playlist.coverUrl,
      subtitle: playlist.ownerUsername,
    })) || [];

  return (
    <div className="account-wrapper">
      <div
        className="profile-header profile-header-bg"
        style={{
          backgroundImage: `url(${image})`,
        }}
      >
        <img
          src={profile.avatarUrl || image}
          alt="userAvatar"
          className="profile-avatar"
        />

        <div className="profile-info-container">
          <p className="profile-username">{profile.username ?? "Username"}</p>
          <p className="profile-stats">
            {profile?.countPlaylist ?? profile?.CountPlaylist ?? 0} playlist{" "}
            <span className="profile-dot">•</span>{" "}
            {profile?.countFollowers ?? profile?.CountFollowers ?? 0} followers
          </p>

          {isOwnProfile ? (
            <button
              onClick={() => setIsEditing(true)}
              className="edit-profile-btn"
            >
              Edit profile
            </button>
          ) : (
            <button
              onClick={handleToggleFollow}
              className="follow-btn"
              style={{
                backgroundColor: profile?.isFollowing ? "#E53935" : "#1DB954",
              }}
            >
              {profile?.isFollowing ? "Unfollow" : "Follow"}
            </button>
          )}
        </div>
      </div>

      <div className="profile-media profile-media-container">
        <Shelf
          title="Recent"
          items={historyItems}
          shape="square"
          loading={loading}
          onSelect={(item) => console.log("Selected track:", item)}
        />

        <Shelf
          title="Playlists"
          items={playlistItems}
          shape="square"
          loading={loading}
          onSelect={(item) => console.log("Selected playlist:", item)}
        />
      </div>

      {isEditing && (
        <EditProfileForm
          profile={profile}
          accessToken={accessToken}
          onClose={() => setIsEditing(false)}
          onUpdateSuccess={(updatedData) => setProfile(updatedData)}
        />
      )}
    </div>
  );
}
