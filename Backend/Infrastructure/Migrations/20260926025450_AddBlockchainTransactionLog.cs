using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchainTransactionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_Playlists_UserId",
            //    table: "Playlists");

            //migrationBuilder.DropIndex(
            //    name: "IX_ListeningHistories_UserId",
            //    table: "ListeningHistories");

            migrationBuilder.CreateTable(
                name: "BlockchainTransactionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RawUserId = table.Column<string>(type: "text", nullable: false),
                    ResolvedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlanType = table.Column<byte>(type: "smallint", nullable: false),
                    Buyer = table.Column<string>(type: "text", nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainTransactionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockchainTransactionLogs_Users_ResolvedUserId",
                        column: x => x.ResolvedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tracks_CreatedAt",
            //    table: "Tracks",
            //    column: "CreatedAt");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tracks_PlaysCount",
            //    table: "Tracks",
            //    column: "PlaysCount");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tracks_Title",
            //    table: "Tracks",
            //    column: "Title");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PlaylistTracks_PlaylistId_AddedAt",
            //    table: "PlaylistTracks",
            //    columns: new[] { "PlaylistId", "AddedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Playlists_Name",
            //    table: "Playlists",
            //    column: "Name");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Playlists_UserId_CreatedAt",
            //    table: "Playlists",
            //    columns: new[] { "UserId", "CreatedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ListeningHistories_UserId_ListenedAt",
            //    table: "ListeningHistories",
            //    columns: new[] { "UserId", "ListenedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_LikedTracks_UserId_LikedAt",
            //    table: "LikedTracks",
            //    columns: new[] { "UserId", "LikedAt" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Artists_Name",
            //    table: "Artists",
            //    column: "Name");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Albums_Title",
            //    table: "Albums",
            //    column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactionLogs_ResolvedUserId",
                table: "BlockchainTransactionLogs",
                column: "ResolvedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainTransactionLogs");

            //migrationBuilder.DropIndex(
            //    name: "IX_Tracks_CreatedAt",
            //    table: "Tracks");

            //migrationBuilder.DropIndex(
            //    name: "IX_Tracks_PlaysCount",
            //    table: "Tracks");

            //migrationBuilder.DropIndex(
            //    name: "IX_Tracks_Title",
            //    table: "Tracks");

            //migrationBuilder.DropIndex(
            //    name: "IX_PlaylistTracks_PlaylistId_AddedAt",
            //    table: "PlaylistTracks");

            //migrationBuilder.DropIndex(
            //    name: "IX_Playlists_Name",
            //    table: "Playlists");

            //migrationBuilder.DropIndex(
            //    name: "IX_Playlists_UserId_CreatedAt",
            //    table: "Playlists");

            //migrationBuilder.DropIndex(
            //    name: "IX_ListeningHistories_UserId_ListenedAt",
            //    table: "ListeningHistories");

            //migrationBuilder.DropIndex(
            //    name: "IX_LikedTracks_UserId_LikedAt",
            //    table: "LikedTracks");

            //migrationBuilder.DropIndex(
            //    name: "IX_Artists_Name",
            //    table: "Artists");

            //migrationBuilder.DropIndex(
            //    name: "IX_Albums_Title",
            //    table: "Albums");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Playlists_UserId",
            //    table: "Playlists",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ListeningHistories_UserId",
            //    table: "ListeningHistories",
            //    column: "UserId");
        }
    }
}
