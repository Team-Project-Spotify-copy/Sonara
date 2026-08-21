using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddCatalogSearchAndLibraryIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_ListeningHistories_UserId",
                table: "ListeningHistories");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_CreatedAt",
                table: "Tracks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_PlaysCount",
                table: "Tracks",
                column: "PlaysCount");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Title",
                table: "Tracks",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTracks_PlaylistId_AddedAt",
                table: "PlaylistTracks",
                columns: new[] { "PlaylistId", "AddedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_Name",
                table: "Playlists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId_CreatedAt",
                table: "Playlists",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistories_UserId_ListenedAt",
                table: "ListeningHistories",
                columns: new[] { "UserId", "ListenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LikedTracks_UserId_LikedAt",
                table: "LikedTracks",
                columns: new[] { "UserId", "LikedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Name",
                table: "Artists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Title",
                table: "Albums",
                column: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_CreatedAt",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_PlaysCount",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_Title",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistTracks_PlaylistId_AddedAt",
                table: "PlaylistTracks");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_Name",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_UserId_CreatedAt",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_ListeningHistories_UserId_ListenedAt",
                table: "ListeningHistories");

            migrationBuilder.DropIndex(
                name: "IX_LikedTracks_UserId_LikedAt",
                table: "LikedTracks");

            migrationBuilder.DropIndex(
                name: "IX_Artists_Name",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Title",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistories_UserId",
                table: "ListeningHistories",
                column: "UserId");
        }
    }
}
