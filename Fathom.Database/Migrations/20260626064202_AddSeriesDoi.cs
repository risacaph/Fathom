using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fathom.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesDoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Doi",
                table: "SeriesMetadata",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Doi",
                table: "SeriesMetadata");
        }
    }
}
