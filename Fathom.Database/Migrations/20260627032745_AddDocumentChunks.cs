using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fathom.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentChunk",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChunkIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Embedding = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EmbeddingModel = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunk_ChapterId",
                table: "DocumentChunk",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunk_LibraryId",
                table: "DocumentChunk",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunk_SeriesId",
                table: "DocumentChunk",
                column: "SeriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentChunk");
        }
    }
}
