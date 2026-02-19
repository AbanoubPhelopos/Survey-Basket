using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTenantRoleClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 15, "permissions", "surveys:audience:assign", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 16, "permissions", "companies:manage", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 17, "permissions", "companies:users:manage", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 18, "permissions", "surveys:analytics:partner", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 19, "permissions", "polls:read", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 20, "permissions", "polls:add", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 21, "permissions", "polls:update", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 22, "permissions", "polls:delete", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 23, "permissions", "surveys:audience:assign", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 24, "permissions", "companies:manage", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 25, "permissions", "companies:users:manage", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 26, "permissions", "results:read", new Guid("0399fd4b-3c1f-7eab-84c4-47f5aad20c88") },
                    { 27, "permissions", "surveys:own:manage", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 28, "permissions", "surveys:analytics:partner", new Guid("0499fd4b-3c1f-7eab-84c4-47f5aad20c89") },
                    { 29, "permissions", "surveys:company:submit", new Guid("0599fd4b-3c1f-7eab-84c4-47f5aad20c8a") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29);
        }
    }
}
