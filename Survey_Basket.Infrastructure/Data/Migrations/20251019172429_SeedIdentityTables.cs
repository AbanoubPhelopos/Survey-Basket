using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"), "1199fd50-a461-715e-ae9c-ddf7d6ea249f", false, false, "Admin", "ADMIN" },
                    { new Guid("0299fd4b-3c1f-7eab-84c4-47f5aad20c87"), "2299fd50-a461-715e-ae9c-ddf7d6ea249f", true, false, "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"), 0, "0199fd50-a461-715e-ae9c-ddf7d6ea249f", "admin@survey-basket.com", true, "System", "Admin", false, null, "ADMIN@SURVEY-BASKET.COM", "ADMIN@SURVEY-BASKET.COM", "AQAAAAIAAYagAAAAEC/kkJc3Or5wPLWGiFYs0xTwNutl8qhETSTWJGaHm37VTdJ6rTGrDHJtBqPihsxdMQ==", null, false, "F66F472946EC4BBB86994AFF718329A7", false, "admin@survey-basket.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permissions", "polls:read", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 2, "permissions", "polls:add", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 3, "permissions", "polls:update", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 4, "permissions", "polls:delete", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 5, "permissions", "questions:read", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 6, "permissions", "questions:add", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 7, "permissions", "questions:update", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 8, "permissions", "users:read", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 9, "permissions", "users:add", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 10, "permissions", "users:update", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 11, "permissions", "roles:read", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 12, "permissions", "roles:add", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 13, "permissions", "roles:update", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") },
                    { 14, "permissions", "results:read", new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"), new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0299fd4b-3c1f-7eab-84c4-47f5aad20c87"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"), new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86") });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"));
        }
    }
}
