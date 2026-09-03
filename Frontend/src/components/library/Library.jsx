import image from "../../assets/images/library-bg.png";
import "../../css/LibraryPage.css";
import { useState } from "react";
import { motion } from "framer-motion";

import useLibrary from "../../hooks/useLibrary.js";
import Shelf from "../media/Shelf.jsx";

export default function Library() {
  const [buttons, setButtons] = useState([
    { key: "all", label: "All" },
    { key: "playlists", label: "Playlists" },
    { key: "podcasts", label: "Podcasts" },
    { key: "albums", label: "Albums" },
    { key: "artists", label: "Artists" },
  ]);

  const [selectedCategory, setSelectedCategory] = useState("all");
  const {
    items: libraryItems = [],
    status: libraryStatus,
    error: libraryError,
  } = useLibrary();

  const handleClick = (clickedKey) => {
    setSelectedCategory(clickedKey);

    setButtons((prevButtons) => {
      const clickedButton = prevButtons.find((btn) => btn.key === clickedKey);
      const otherButtons = prevButtons.filter((btn) => btn.key !== clickedKey);
      return [clickedButton, ...otherButtons];
    });
  };

  const categoryMap = {
    playlists: "playlist",
    podcasts: "podcast",
    albums: "album",
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
  const albums = libraryItems.filter((item) => item.kind === "album");
  const artists = libraryItems.filter((item) => item.kind === "artist");

  return (
    <div
      className="library-container"
      style={{ backgroundImage: `url(${image})` }}
    >
      <h1 className="library-title">Library</h1>

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
                onSelect={() => {}}
              />
            )}
            {podcasts.length > 0 && (
              <Shelf
                key={"shelf-podcasts"}
                title={"Podcasts"}
                shape={"square"}
                items={podcasts}
                loading={false}
                onSelect={() => {}}
              />
            )}
            {albums.length > 0 && (
              <Shelf
                key={"shelf-albums"}
                title={"Albums"}
                shape={"square"}
                items={albums}
                loading={false}
                onSelect={() => {}}
              />
            )}
            {artists.length > 0 && (
              <Shelf
                key={"shelf-artists"}
                title={"Artists"}
                shape={"square"}
                items={artists}
                loading={false}
                onSelect={() => {}}
              />
            )}
          </>
        ) : (
          <Shelf
            key={`shelf-${selectedCategory}`}
            title={buttons.find((b) => b.key === selectedCategory)?.label}
            shape={"square"}
            items={filteredItems}
            loading={false}
            onSelect={() => {}}
          />
        )}
      </div>
    </div>
  );
}
