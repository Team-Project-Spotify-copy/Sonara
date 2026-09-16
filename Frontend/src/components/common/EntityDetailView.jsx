import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { AccountContext } from "@contexts/account.store";
import EntityDetailViewItem from "@components/common/EntityDetailViewItem";
import image from "@assets/images/subscription-hd-bg.png";
import axios from "axios";
import { usePlayer } from "@contexts/player.store";
import AddEntityModal from "@components/common/AddEntityModal";
import "@css/EntityDetailView.css";

export default function EntityDetailView({ type, apiConfig }) {
  const { accessToken } = React.useContext(AccountContext);
  const [entity, setEntity] = useState({});
  const [items, setItems] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { setQueueAndPlay } = usePlayer();
  const api = import.meta.env.VITE_API;
  const { name } = useParams();

  const fetchEntityData = async () => {
    try {
      const response = await axios.get(
        `${api}/${apiConfig.baseRoute}/${name}`,
        {
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      if (response.status === 200 && response.data) {
        setEntity(response.data);

        if (response.data.tracks) {
          setItems(response.data.tracks);
        } else if (response.data.episodes) {
          setItems(response.data.episodes);
        }
      }
    } catch (error) {
      console.error(`Error fetching ${type}:`, error);
    }
  };

  const fetchTracks = async () => {
    try {
      const response = await axios.get(
        `${api}/${apiConfig.baseRoute}/${entity.id}/tracks`,
        {
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      if (response.status === 200 && response.data) {
        const mappedTracks = response.data.map((item) => item.track);
        setItems(mappedTracks);
      }
    } catch (error) {
      console.error(`Error fetching ${type} tracks:`, error);
    }
  };

  useEffect(() => {
    if (name) {
      fetchEntityData();
    }
  }, [name]);

  useEffect(() => {
    if (entity?.id && type === "Playlist") {
      fetchTracks();
    }
  }, [entity]);

  const handleItemAdded = () => {
    fetchEntityData();
    if (type === "Playlist") {
      fetchTracks();
    }
  };

  const totalMinutes = entity.totalDurationMs
    ? entity.totalDurationMs / 60000
    : 0;
  const hours = Math.floor(totalMinutes / 60);
  const minutes = Math.floor(totalMinutes % 60);

  return (
    <div className="entity-detail-container">
      <div
        className="entity-detail-header"
        style={{ backgroundImage: `url(${image})` }}
      >
        <img
          src={entity.coverUrl || entity.imageUrl}
          alt={`${type} Header`}
          className="entity-header-cover"
        />
        <div>
          <p className="entity-header-title">
            {entity.name || entity.title || `${type} Name`}
          </p>
          <p className="entity-header-subtitle">
            {entity.ownerUsername ||
              entity.artistName ||
              entity.subtitle ||
              "Creator"}{" "}
            <span className="profile-dot">•</span>{" "}
            {hours > 0 ? `${hours} h ${minutes} min` : `${minutes} min`}
          </p>
        </div>
      </div>

      <div className="entity-detail-content">
        <div className="entity-action-bar">
          <div className="entity-action-circle"></div>
          <div className="entity-action-circle"></div>
          <div className="entity-action-circle"></div>

          <button
            onClick={() => setIsModalOpen(true)}
            className="entity-add-track-btn"
          >
            <svg
              className="create-plus-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              strokeLinecap="round"
            >
              <path d="M12 4v16m-8-8h16" />
            </svg>
            {type !== "Podcast" ? "Add Track" : "Add Episode"}
          </button>

          <div className="entity-action-circle"></div>
        </div>

        <div className="entity-list-header-wrapper">
          <div className="entity-list-header-row">
            <p className="entity-list-header-text"># Name</p>
            <div className="entity-list-header-right">
              <p className="entity-list-header-text">Date added</p>
              <p className="entity-list-header-text">Time</p>
            </div>
          </div>
          <hr className="entity-list-divider" />
        </div>

        <div className="entity-tracks-list">
          {items.map((item, index) => {
            const durationSec =
              item.durationSeconds ||
              (item.durationMs ? Math.floor(item.durationMs / 1000) : 0);

            return (
              <div
                key={index}
                onClick={() => {
                  setQueueAndPlay(items, index, { autoplay: true });
                }}
                className="entity-track-item-wrapper"
              >
                <EntityDetailViewItem
                  countPosition={index + 1}
                  CoverUrl={
                    item.artworkUrl ||
                    item.coverUrl ||
                    entity.coverUrl ||
                    entity.imageUrl
                  }
                  Name={item.title}
                  Artist={
                    item.artistName || entity.authorName || entity.subtitle
                  }
                  date={item.createdAt || item.releaseDate}
                  duration={durationSec}
                />
              </div>
            );
          })}
        </div>
      </div>

      {isModalOpen && apiConfig.addEntityEndpoint && (
        <AddEntityModal
          entityId={entity.id}
          entityType={type}
          onClose={() => setIsModalOpen(false)}
          onSuccess={handleItemAdded}
          apiEndpointBuilder={apiConfig.addEntityEndpoint}
        />
      )}
    </div>
  );
}
