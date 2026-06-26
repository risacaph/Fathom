using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fathom.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupersededBySeriesId",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Series",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "SupersededBySeriesId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Series");
        }
    }
}
