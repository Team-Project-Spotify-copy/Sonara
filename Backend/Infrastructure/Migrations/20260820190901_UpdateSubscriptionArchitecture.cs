using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Subscriptions_SubscriptionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Users_SubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveSubscriptionId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    MaxSlots = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "Features", "MaxSlots", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-111111111111"), "Ads included, Audio standard quality, Shuffle only", 1, "Free", 0.00m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "No ads, High quality audio, Offline downloads", 1, "Individual", 3.99m },
                    { new Guid("22222222-2222-2222-2222-333333333333"), "Up to 2 accounts, Explicit filter, Shared mix", 2, "Duo", 6.99m },
                    { new Guid("22222222-2222-2222-2222-444444444444"), "Up to 6 accounts, Explicit filter, Shared mix", 6, "Family", 9.99m }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-111111111111"),
                column: "ActiveSubscriptionId",
                value: new Guid("33333333-3333-3333-9999-111111111111"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-222222222222"),
                column: "ActiveSubscriptionId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ActiveSubscriptionId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-444444444444"),
                column: "ActiveSubscriptionId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-555555555555"),
                column: "ActiveSubscriptionId",
                value: null);

            migrationBuilder.InsertData(
                table: "UserSubscriptions",
                columns: new[] { "Id", "ExpiresAt", "OwnerId", "PlanId" },
                values: new object[] { new Guid("33333333-3333-3333-9999-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-111111111111"), new Guid("22222222-2222-2222-2222-333333333333") });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveSubscriptionId",
                table: "Users",
                column: "ActiveSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_OwnerId",
                table: "UserSubscriptions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserSubscriptions_ActiveSubscriptionId",
                table: "Users",
                column: "ActiveSubscriptionId",
                principalTable: "UserSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserSubscriptions_ActiveSubscriptionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveSubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveSubscriptionId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "Features", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-111111111111"), "Ads included, Audio standard quality, Shuffle only", "Free", 0.00m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "No ads, High quality audio, Offline downloads", "Premium", 4.99m },
                    { new Guid("22222222-2222-2222-2222-333333333333"), "Up to 6 accounts, Explicit filter, Shared mix", "Family", 7.99m }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-111111111111"),
                column: "SubscriptionId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-222222222222"),
                column: "SubscriptionId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "SubscriptionId",
                value: new Guid("22222222-2222-2222-2222-333333333333"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-444444444444"),
                column: "SubscriptionId",
                value: new Guid("22222222-2222-2222-2222-111111111111"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-555555555555"),
                column: "SubscriptionId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubscriptionId",
                table: "Users",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Subscriptions_SubscriptionId",
                table: "Users",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
