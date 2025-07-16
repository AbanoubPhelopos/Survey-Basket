using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumnsToPollTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Polls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById1",
                table: "Polls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Polls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Polls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById1",
                table: "Polls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Polls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Polls_CreatedById1",
                table: "Polls",
                column: "CreatedById1");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_UpdatedById1",
                table: "Polls",
                column: "UpdatedById1");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls",
                column: "CreatedById1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_UpdatedById1",
                table: "Polls",
                column: "UpdatedById1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls");

            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_UpdatedById1",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_Polls_CreatedById1",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_Polls_UpdatedById1",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "CreatedById1",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "UpdatedById1",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Polls");
        }
    }
}
