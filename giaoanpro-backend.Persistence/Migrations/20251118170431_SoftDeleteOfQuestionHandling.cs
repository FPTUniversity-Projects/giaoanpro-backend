using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteOfQuestionHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "QuestionOptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "QuestionAttributes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ExamQuestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AttemptDetails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "QuestionAttributes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AttemptDetails");
        }
    }
}
