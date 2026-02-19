using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Survey_Basket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorVoteAnswersForTypedSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteAnswers_Answers_AnswerId",
                table: "VoteAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AnswerId",
                table: "VoteAnswers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "BoolValue",
                table: "VoteAnswers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "VoteAnswers",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileReference",
                table: "VoteAnswers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumberValue",
                table: "VoteAnswers",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedOptionIdsJson",
                table: "VoteAnswers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextValue",
                table: "VoteAnswers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 36, "permissions", "polls:read", new Guid("0299fd4b-3c1f-7eab-84c4-47f5aad20c87") },
                    { 37, "permissions", "surveys:company:submit", new Guid("0299fd4b-3c1f-7eab-84c4-47f5aad20c87") }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_VoteAnswers_Answers_AnswerId",
                table: "VoteAnswers",
                column: "AnswerId",
                principalTable: "Answers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteAnswers_Answers_AnswerId",
                table: "VoteAnswers");

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DropColumn(
                name: "BoolValue",
                table: "VoteAnswers");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "VoteAnswers");

            migrationBuilder.DropColumn(
                name: "FileReference",
                table: "VoteAnswers");

            migrationBuilder.DropColumn(
                name: "NumberValue",
                table: "VoteAnswers");

            migrationBuilder.DropColumn(
                name: "SelectedOptionIdsJson",
                table: "VoteAnswers");

            migrationBuilder.DropColumn(
                name: "TextValue",
                table: "VoteAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AnswerId",
                table: "VoteAnswers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteAnswers_Answers_AnswerId",
                table: "VoteAnswers",
                column: "AnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
