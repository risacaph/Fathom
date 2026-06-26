using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fathom.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchFts5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Full-text search index over book/PDF content. Managed via raw SQL (not an EF entity).
            // ChapterId/SeriesId/LibraryId are UNINDEXED so they're stored for filtering/lookup but
            // do not participate in the full-text match. e_sqlite3 ships with FTS5 enabled.
            migrationBuilder.Sql(@"
                CREATE VIRTUAL TABLE IF NOT EXISTS ""ChapterFts"" USING fts5(
                    Title,
                    Content,
                    ChapterId UNINDEXED,
                    SeriesId UNINDEXED,
                    LibraryId UNINDEXED,
                    tokenize = 'porter unicode61'
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""ChapterFts"";");
        }
    }
}
