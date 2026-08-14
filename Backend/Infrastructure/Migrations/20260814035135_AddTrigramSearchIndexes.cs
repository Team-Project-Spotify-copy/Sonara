using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddTrigramSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Tracks_Title_Trgm""
                  ON ""Tracks"" USING gin (lower(""Title"") gin_trgm_ops);");

            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Albums_Title_Trgm""
                  ON ""Albums"" USING gin (lower(""Title"") gin_trgm_ops);");

            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Artists_Name_Trgm""
                  ON ""Artists"" USING gin (lower(""Name"") gin_trgm_ops);");

            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Playlists_Name_Trgm""
                  ON ""Playlists"" USING gin (lower(""Name"") gin_trgm_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Playlists_Name_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Artists_Name_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Albums_Title_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Tracks_Title_Trgm"";");

        }
    }
}
