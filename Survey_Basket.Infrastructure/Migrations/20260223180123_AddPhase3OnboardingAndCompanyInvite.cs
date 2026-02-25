using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3OnboardingAndCompanyInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileCompleted",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CompanyMagicLinkTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMagicLinkTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyMagicLinkTokens_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyMagicLinkTokens_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyMagicLinkTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUserInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    EmailHint = table.Column<string>(type: "text", nullable: true),
                    MobileHint = table.Column<string>(type: "text", nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUserInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyUserInvites_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyUserInvites_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyUserInvites_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("0199fd4b-3c1f-7eab-84c4-47f5aad20c86"),
                columns: new[] { "IsFirstLogin", "ProfileCompleted" },
                values: new object[] { false, false });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMagicLinkTokens_CreatedById",
                table: "CompanyMagicLinkTokens",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMagicLinkTokens_UpdatedById",
                table: "CompanyMagicLinkTokens",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMagicLinkTokens_UserId",
                table: "CompanyMagicLinkTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUserInvites_CompanyId",
                table: "CompanyUserInvites",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUserInvites_CreatedById",
                table: "CompanyUserInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUserInvites_UpdatedById",
                table: "CompanyUserInvites",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyMagicLinkTokens");

            migrationBuilder.DropTable(
                name: "CompanyUserInvites");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileCompleted",
                table: "AspNetUsers");
        }
    }
}
