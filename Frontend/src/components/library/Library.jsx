import image from "../../assets/images/library-bg.png";
import "../../css/LibraryPage.css";
import { useState } from "react";
import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import useLibrary from "../../hooks/useLibrary.js";
import Shelf from "../media/Shelf.jsx";
import AddEntityModal from "./AddEntityModal.jsx";

export default function Library() {
  const navigate = useNavigate();
  const [buttons, setButtons] = useState([
    { key: "all", label: "All" },
    { key: "playlists", label: "Playlists" },
    { key: "podcasts", label: "Podcasts" },
    { key: "artists", label: "Artists" },
  ]);

  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [activeModal, setActiveModal] = useState(null);

  const [selectedCategory, setSelectedCategory] = useState("all");
  const {
    items: libraryItems = [],
    status: libraryStatus,
    error: libraryError,
  } = useLibrary();

  const handleAddOptionSelect = (type) => {
    setIsMenuOpen(false);
    setActiveModal(type);
  };

  const handleClick = (clickedKey) => {
    setSelectedCategory(clickedKey);

    setButtons((prevButtons) => {
      const clickedButton = prevButtons.find((btn) => btn.key === clickedKey);
      const otherButtons = prevButtons.filter((btn) => btn.key !== clickedKey);
      return [clickedButton, ...otherButtons];
    });
  };

  const handleItemSelect = (item) => {
    navigate(
      `/${item.kind === "artist" ? "account" : item.kind}/${item.kind === "artist" ? item.RouteKey : item.id}`,
    );
  };

  const categoryMap = {
    playlists: "playlist",
    podcasts: "podcast",
    artists: "artist",
  };

  const filteredItems =
    selectedCategory === "all"
      ? libraryItems
      : libraryItems.filter(
          (item) => item.kind === categoryMap[selectedCategory],
        );

  const playlists = libraryItems.filter((item) => item.kind === "playlist");
  const podcasts = libraryItems.filter((item) => item.kind === "podcast");
  const artists = libraryItems.filter((item) => item.kind === "artist");

  return (
    <div
      className="library-container"
      style={{ backgroundImage: `url(${image})` }}
    >
      <div className="library-header-top">
        <h1 className="library-title">Library</h1>

        <div className="library-create-wrapper">
          <button
            className="btn-create-library"
            onClick={() => setIsMenuOpen((prev) => !prev)}
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
            </svg>{" "}
            Create
          </button>

          {isMenuOpen && (
            <div className="create-dropdown-menu">
              <button onClick={() => handleAddOptionSelect("playlist")}>
                New Playlist
              </button>
              <button onClick={() => handleAddOptionSelect("podcast")}>
                Add Podcast
              </button>
              <button onClick={() => handleAddOptionSelect("artist")}>
                Follow Artist
              </button>
            </div>
          )}
        </div>
      </div>

      <div className="library-buttons">
        {buttons.map((btn) => {
          const isSelected = btn.key === selectedCategory;

          return (
            <motion.button
              key={btn.key}
              layout
              transition={{ type: "spring", stiffness: 300, damping: 40 }}
              onClick={() => handleClick(btn.key)}
              className={`btn-category ${isSelected ? "selected" : ""}`}
            >
              {btn.label}
            </motion.button>
          );
        })}
      </div>

      <div className="library-shelves">
        {libraryStatus === "loading" ? (
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            transition={{
              duration: 0.3,
              repeat: Infinity,
              repeatType: "reverse",
            }}
            style={{
              display: "flex",
              justifyContent: "center",
              alignItems: "center",
              fontFamily: "Inter",
              fontSize: "32px",
              height: "200px",
              color: "#fff",
              fontWeight: "500",
            }}
          >
            Loading...
          </motion.div>
        ) : selectedCategory === "all" ? (
          <>
            {playlists.length > 0 && (
              <Shelf
                key={"shelf-playlists"}
                title={"Playlists"}
                shape={"square"}
                items={playlists}
                loading={false}
                onSelect={handleItemSelect}
              />
            )}
            {podcasts.length > 0 && (
              <Shelf
                key={"shelf-podcasts"}
                title={"Podcasts"}
                shape={"square"}
                items={podcasts}
                loading={false}
                onSelect={handleItemSelect}
              />
            )}
            {artists.length > 0 && (
              <Shelf
                key={"shelf-artists"}
                title={"Artists"}
                shape={"round"}
                items={artists}
                loading={false}
                onSelect={handleItemSelect}
              />
            )}
          </>
        ) : (
          <Shelf
            key={`shelf-${selectedCategory}`}
            title={buttons.find((b) => b.key === selectedCategory)?.label}
            shape={selectedCategory === "artists" ? "round" : "square"}
            items={filteredItems}
            loading={false}
            onSelect={handleItemSelect}
          />
        )}

        {activeModal && (
          <AddEntityModal
            type={activeModal}
            onClose={() => setActiveModal(null)}
            onSuccess={(newItem) => {
              console.log("Успішно створено/додано:", newItem);
              setActiveModal(null);
              window.location.reload();
            }}
          />
        )}
      </div>
    </div>
  );
}
