using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePollTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Polls",
                newName: "Summary");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndedAt",
                table: "Polls",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Polls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartedAt",
                table: "Polls",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Polls");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "Polls",
                newName: "Description");
        }
    }
}
