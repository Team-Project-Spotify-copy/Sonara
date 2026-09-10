import React from "react";
import EntityDetailView from "../common/EntityDetailView";

export default function AlbumPage() {
  const config = {
    baseRoute: "albums",
    addEntityEndpoint: (id, elementName) =>
      `https://localhost:7083/api/albums/${id}/tracks?trackName=${elementName}`,
  };

  return <EntityDetailView type="Album" apiConfig={config} />;
}
