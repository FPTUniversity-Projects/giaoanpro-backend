using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserRole_Activity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionBanks_BankId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionBanks");

            migrationBuilder.DropIndex(
                name: "IX_Questions_BankId",
                table: "Questions");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Activitys",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activitys_ParentId",
                table: "Activitys",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activitys_Activitys_ParentId",
                table: "Activitys",
                column: "ParentId",
                principalTable: "Activitys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activitys_Activitys_ParentId",
                table: "Activitys");

            migrationBuilder.DropIndex(
                name: "IX_Activitys_ParentId",
                table: "Activitys");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Activitys");

            migrationBuilder.CreateTable(
                name: "QuestionBanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBanks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_BankId",
                table: "Questions",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionBanks_BankId",
                table: "Questions",
                column: "BankId",
                principalTable: "QuestionBanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
