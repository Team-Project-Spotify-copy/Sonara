using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Триграмні GIN-індекси під пошук по підрядку (lower(column) LIKE '%term%'), який
    /// звичайний btree прискорити не може. Індекси описані сирим SQL, бо вирази
    /// lower(...) + gin_trgm_ops не мають відповідника в моделі EF; знімок моделі про них
    /// не знає, і подальші міграції їх не чіпатимуть.
    ///
    /// Міграція винесена окремо навмисно: CREATE EXTENSION потребує підвищених прав.
    /// Якщо у користувача БД їх немає, розгортайте до попередньої міграції
    /// (dotnet ef database update AddCatalogSearchAndLibraryIndexes) - пошук працюватиме
    /// коректно й без цих індексів, лише повільніше на великих обсягах.
    /// </summary>
    public partial class AddTrigramSearchIndexes : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Playlists_Name_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Artists_Name_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Albums_Title_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Tracks_Title_Trgm"";");

            // Розширення pg_trgm навмисно не видаляється: ним можуть користуватися інші обʼєкти.
        }
    }
}
