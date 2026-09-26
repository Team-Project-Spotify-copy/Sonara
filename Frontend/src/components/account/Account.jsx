import React, { useState, useEffect } from "react";
import { AccountContext } from "@contexts/account.store";
import { useParams } from "react-router-dom";
import axios from "axios";
import image from "@assets/images/subscription-hd-bg.png";
import avatar from "@assets/images/standart-avatar.png";
import Shelf from "@components/media/Shelf";
import AccentPattern from "@components/common/AccentPattern";
import EditProfileForm from "@components/account/EditProfileForm";
import useRecommendations from "@hooks/useRecommendations";
import useDominantColor from "@hooks/useDominantColor";
import "@css/Account.css";

export default function Account({ onSelect, onLibraryChange }) {
  const { accessToken } = React.useContext(AccountContext);
  const [profile, setProfile] = useState({});
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const { username } = useParams();

  const api = import.meta.env.VITE_API;
  const isOwnProfile = !username;

  const { items: recommendedItems, status: recommendedStatus } =
    useRecommendations({
      enabled: isOwnProfile,
    });

  const avatarSrc = profile.avatarUrl || avatar;
  const accent = useDominantColor(avatarSrc);

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
        console.log(response.data);
        setProfile(response.data);
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

      if (onLibraryChange) {
        onLibraryChange();
      }
    } catch (error) {
      console.error("Error toggling follow state:", error);
      GetAccountUser();
    }
  }

  const historyItems =
    profile?.history?.map((item) => ({
      id: item.track?.id,
      title: item.track?.title || "Unknown Track",
      name: item.track?.title || "Unknown Track",
      imageUrl: item.track?.artworkUrl,
      coverUrl: item.track?.artworkUrl,
      listenedAt: item.listenedAt,
      kind: "track",
    })) || [];

  const playlistItems =
    profile?.playlists?.map((playlist) => ({
      id: playlist.id,
      title: playlist.name,
      name: playlist.name,
      imageUrl: playlist.coverUrl,
      coverUrl: playlist.coverUrl,
      subtitle: playlist.ownerUsername,
      kind: "playlist",
    })) || [];

  const albumItems =
    profile?.albums?.map((album) => ({
      id: album.id,
      title: album.title,
      name: album.title,
      imageUrl: album.coverUrl,
      coverUrl: album.coverUrl,
      subtitle: album.artistName,
      kind: "album",
    })) || [];

  return (
    <div className="account-wrapper">
      <div className="profile-header profile-header-bg">
        <AccentPattern
          className="profile-header__backdrop"
          image={image}
          accent={accent}
        />

        <img src={avatarSrc} alt="userAvatar" className="profile-avatar" />

        <div className="profile-info-container">
          <p className="profile-username">
            {profile.username ?? "Username"}
          </p>
          <p className="profile-stats">
            {profile?.countAlbum ?? profile?.countAlbum ?? 0} album
            <span className="profile-dot">•</span>{" "}
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
        {isOwnProfile && (
          <Shelf
            title="Recommended for you"
            items={recommendedItems}
            shape="square"
            loading={recommendedStatus === "loading"}
            onSelect={onSelect}
          />
        )}

        <Shelf
          title="Albums"
          items={albumItems}
          shape="square"
          loading={loading}
          onSelect={onSelect}
        />

        <Shelf
          title="Playlists"
          items={playlistItems}
          shape="square"
          loading={loading}
          onSelect={onSelect}
        />

        <Shelf
          title="Recent"
          items={historyItems}
          shape="square"
          loading={loading}
          onSelect={onSelect}
        />
      </div>

      {isEditing && (
        <EditProfileForm
          profile={profile}
          accessToken={accessToken}
          onClose={() => setIsEditing(false)}
          onUpdateSuccess={(updatedData) => {
            setProfile(updatedData);

            if (onLibraryChange) {
              onLibraryChange();
            }
          }}
        />
      )}
    </div>
  );
}
