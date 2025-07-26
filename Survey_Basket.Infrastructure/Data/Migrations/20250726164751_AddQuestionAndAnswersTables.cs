using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionAndAnswersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls");

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById1 = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_AspNetUsers_CreatedById1",
                        column: x => x.CreatedById1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_AspNetUsers_UpdatedById1",
                        column: x => x.UpdatedById1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Questions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId_Content",
                table: "Answers",
                columns: new[] { "QuestionId", "Content" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedById1",
                table: "Questions",
                column: "CreatedById1");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PollId_Content",
                table: "Questions",
                columns: new[] { "PollId", "Content" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UpdatedById1",
                table: "Questions",
                column: "UpdatedById1");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls",
                column: "CreatedById1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedById1",
                table: "Polls",
                column: "CreatedById1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
