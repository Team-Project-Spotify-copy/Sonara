import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { AccountContext } from "../../contexts/account.store";
import TrackItem from "./TrackItem";
import image from "../../assets/images/playlist-header-bg.png";
import axios from "axios";
import { usePlayer } from "../../contexts/player.store";
import AddTrackModal from "./AddTrackModal";

export default function Playlist() {
  const { accessToken } = React.useContext(AccountContext);
  const [playlist, setPlaylist] = useState([]);
  const [playlistTracks, setPlaylistTracks] = useState([]);
  const [isAddTrackModalOpen, setIsAddTrackModalOpen] = useState(false);
  const { setQueueAndPlay } = usePlayer();
  const api = import.meta.env.VITE_API;
  const { name } = useParams();

  const fetchPlaylists = async () => {
    try {
      const response = await axios.get(`${api}/playlists/${name}`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      const data = response.data;

      if (response.status === 200 && response.data) {
        console.log(data);
        setPlaylist(data);
      } else {
        console.error("Error fetching user data");
      }
    } catch (error) {
      console.error("Error fetching playlist:", error);
    }
  };

  const fetchPlaylistsTracks = async () => {
    try {
      const response = await axios.get(
        `${api}/playlists/${playlist.id}/tracks`,
        {
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      const data = response.data;

      if (response.status === 200 && response.data) {
        const tracks = data.map((item) => item.track);
        setPlaylistTracks(tracks);
      } else {
        console.error("Error fetching user data");
      }
    } catch (error) {
      console.error("Error fetching playlists:", error);
    }
  };

  useEffect(() => {
    if (name) {
      fetchPlaylists();
    }
  }, [name]);

  useEffect(() => {
    if (playlist?.id) {
      fetchPlaylistsTracks();
    }
  }, [playlist]);

  const handleTrackAdded = () => {
    fetchPlaylists();
    fetchPlaylistsTracks();
  };

  const totalMinutes = playlist.totalDurationMs / 60000;
  const hours = Math.floor(totalMinutes / 60);
  const minutes = Math.floor(totalMinutes % 60);
  return (
    <div className="playlist-container">
      <div
        className="playlist-header"
        style={{
          backgroundImage: `url(${image})`,
          backgroundSize: "cover",
          backgroundPosition: "center",
          width: "100%",
          height: "364px",
          display: "flex",
          alignItems: "center",
          borderRadius: "8px",
          marginBottom: "20px",
        }}
      >
        <img
          src={playlist.coverUrl}
          alt="Playlist Header"
          style={{
            width: "300px",
            height: "300px",
            objectFit: "cover",
            borderRadius: "8px",
            marginLeft: "32px",
          }}
        />
        <div>
          <p
            style={{
              color: "white",
              margin: "0px 0px 0px 50px",
              fontFamily: "Inter",
              fontSize: "52px",
              fontWeight: "700",
            }}
          >
            {playlist.name || "Playlist Name"}
          </p>
          <p
            style={{
              color: "#B0B0B0",
              margin: "10px 0px 0px 50px",
              fontFamily: "Inter",
              fontSize: "20px",
              fontWeight: "400",
            }}
          >
            {playlist.ownerUsername || "Artist"}{" "}
            <span className="profile-dot">•</span>{" "}
            {hours > 0
              ? `${hours}` + " h " + `${minutes}` + " min"
              : `${minutes}` + " min"}
          </p>
        </div>
      </div>

      <div
        className="playlist-content"
        style={{
          display: "flex",
          flexDirection: "column",
          width: "100%",
          backgroundColor: "#1B1B1B",
          borderRadius: "15px",
          padding: "20px",
          gap: "20px",
          boxSizing: "border-box",
        }}
      >
        <div
          style={{
            display: "flex",
            flexDirection: "row",
            gap: "20px",
            alignItems: "center",
            width: "100%",
          }}
        >
          <div
            style={{
              backgroundColor: "#504F4F",
              width: "60px",
              height: "60px",
              borderRadius: "50%",
            }}
          ></div>

          <div
            style={{
              backgroundColor: "#504F4F",
              width: "60px",
              height: "60px",
              borderRadius: "50%",
            }}
          ></div>

          <div
            style={{
              backgroundColor: "#504F4F",
              width: "60px",
              height: "60px",
              borderRadius: "50%",
            }}
          ></div>

          <button
            onClick={() => setIsAddTrackModalOpen(true)}
            style={{
              borderRadius: "76px",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: "8px",
              width: "182px",
              height: "60px",
              border: "none",
              fontSize: "20px",
              fontWeight: "500",
              fontFamily: "Inter",
              backgroundColor: "#504F4F",
              color: "white",
              marginLeft: "auto",
              cursor: "pointer",
            }}
          >
            <svg
              className="create-plus-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              strokeLinecap="round"
              style={{ width: "20px", height: "20px" }}
            >
              <path d="M12 4v16m-8-8h16" />
            </svg>
            Add Track
          </button>

          <div
            style={{
              backgroundColor: "#504F4F",
              width: "60px",
              height: "60px",
              borderRadius: "50%",
            }}
          ></div>
        </div>

        <div
          style={{
            display: "flex",
            flexDirection: "column",
            width: "100%",
            color: "#B0B0B0",
            fontFamily: "Inter",
          }}
        >
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              paddingBottom: "10px",
            }}
          >
            <p style={{ margin: 0 }}># Name</p>

            <div style={{ display: "flex", gap: "120px" }}>
              <p style={{ margin: 0 }}>Date added</p>
              <p style={{ margin: 0 }}>Time</p>
            </div>
          </div>
          <hr
            style={{ border: "0.5px solid #333", width: "100%", margin: 0 }}
          />
        </div>

        <div
          className="tracks"
          style={{
            display: "flex",
            flexDirection: "column",
            gap: "12px",
          }}
        >
          {playlistTracks.map((track, index) => (
            <div
              key={index}
              onClick={() => {
                setQueueAndPlay(playlistTracks, index, { autoplay: true });
              }}
              style={{ cursor: "pointer" }}
            >
              <TrackItem
                countPosition={index + 1}
                CoverUrl={track.artworkUrl}
                Name={track.title}
                Artist={track.artistName}
                date={track.createdAt}
                duration={track.durationSeconds}
              />
            </div>
          ))}
        </div>
      </div>

      {isAddTrackModalOpen && (
        <AddTrackModal
          playlistId={playlist.id}
          onClose={() => setIsAddTrackModalOpen(false)}
          onSuccess={handleTrackAdded}
        />
      )}
    </div>
  );
}
