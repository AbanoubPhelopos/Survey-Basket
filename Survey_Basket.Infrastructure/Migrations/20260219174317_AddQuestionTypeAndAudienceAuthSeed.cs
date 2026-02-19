using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTypeAndAudienceAuthSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Questions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 30, "permissions", "polls:read", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 31, "permissions", "polls:add", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 32, "permissions", "polls:update", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 33, "permissions", "questions:add", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 34, "permissions", "questions:update", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 35, "permissions", "polls:read", new Guid("0599fd4b-3c1f-7eab-84c4-47f5aad20c8a") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Questions");
        }
    }
}
