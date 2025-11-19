using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class updateQuest : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Questions_PromptLogs_PromptId",
				table: "Questions");

			migrationBuilder.DropIndex(
				name: "IX_Questions_PromptId",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "PromptId",
				table: "Questions");

			migrationBuilder.AddColumn<string>(
				name: "AwarenessLevel",
				table: "Questions",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<DateTime>(
				name: "DeletedAt",
				table: "Questions",
				type: "datetime2",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "DifficultyLevel",
				table: "Questions",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<Guid>(
				name: "PromptLogId",
				table: "Questions",
				type: "uniqueidentifier",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "QuestionType",
				table: "Questions",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AlterColumn<int>(
				name: "Type",
				table: "Attributes",
				type: "int",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)");

			migrationBuilder.CreateIndex(
				name: "IX_Questions_PromptLogId",
				table: "Questions",
				column: "PromptLogId");

			migrationBuilder.AddForeignKey(
				name: "FK_Questions_PromptLogs_PromptLogId",
				table: "Questions",
				column: "PromptLogId",
				principalTable: "PromptLogs",
				principalColumn: "Id");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Questions_PromptLogs_PromptLogId",
				table: "Questions");

			migrationBuilder.DropIndex(
				name: "IX_Questions_PromptLogId",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "AwarenessLevel",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "DeletedAt",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "DifficultyLevel",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "PromptLogId",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "QuestionType",
				table: "Questions");

			migrationBuilder.AddColumn<Guid>(
				name: "PromptId",
				table: "Questions",
				type: "uniqueidentifier",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.AlterColumn<string>(
				name: "Type",
				table: "Attributes",
				type: "nvarchar(max)",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.CreateIndex(
				name: "IX_Questions_PromptId",
				table: "Questions",
				column: "PromptId");

			migrationBuilder.AddForeignKey(
				name: "FK_Questions_PromptLogs_PromptId",
				table: "Questions",
				column: "PromptId",
				principalTable: "PromptLogs",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
