import EntityDetailView from "@components/common/EntityDetailView";

export default function PlaylistPage() {
  const config = {
    baseRoute: "playlists",
    addEntityEndpoint: (id, elementName) =>
      `https://localhost:7083/api/playlists/${id}/tracks?trackName=${elementName}`,
  };

  return <EntityDetailView type="Playlist" apiConfig={config} />;
}
