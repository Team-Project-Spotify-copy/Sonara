import React from "react";
import EntityDetailView from "../common/EntityDetailView";

export default function PodcastPage() {
  const config = {
    baseRoute: "podcasts",
    addEntityEndpoint: (id, elementName) =>
      `https://localhost:7083/api/podcasts/${id}/episodes?episodeName=${elementName}`,
  };

  return <EntityDetailView type="Podcast" apiConfig={config} />;
}
