using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocsToLessonPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Docs",
                table: "LessonPlans",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MatrixId",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Docs",
                table: "LessonPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatrixId",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
