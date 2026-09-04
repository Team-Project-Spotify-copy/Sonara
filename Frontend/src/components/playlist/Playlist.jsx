import React from 'react'
import image from "../../assets/images/playlist-header-bg.png";

export default function Playlist() {
  return (
    <div className="playlist-container">
      <div className="playlist-header" 
      style={{ 
        backgroundImage: `url(${image})`, 
        backgroundSize: "cover", 
        backgroundPosition: "center", 
        width: "100%", 
        height: "364px", 
        display: "flex", 
        alignItems: "center", 
        justifyContent: "center"
         }}>
        
      </div>
        <div className="playlist-content">
      </div>
    </div>
  );
}
