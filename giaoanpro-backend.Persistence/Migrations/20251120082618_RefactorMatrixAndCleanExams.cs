using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMatrixAndCleanExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamMatrices_LessonPlans_LessonPlanId",
                table: "ExamMatrices");

            migrationBuilder.DropTable(
                name: "ExamMatrixLines");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "ExamMatrices");

            migrationBuilder.DropColumn(
                name: "MarksPerQuestion",
                table: "ExamMatrices");

            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "ExamMatrices",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NumberOfQuestions",
                table: "ExamMatrices",
                newName: "TotalQuestions");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatrixId",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExamMatrixDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMatrixDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMatrixDetails_ExamMatrices_MatrixId",
                        column: x => x.MatrixId,
                        principalTable: "ExamMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamMatrixDetails_MatrixId",
                table: "ExamMatrixDetails",
                column: "MatrixId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMatrices_LessonPlans_LessonPlanId",
                table: "ExamMatrices",
                column: "LessonPlanId",
                principalTable: "LessonPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamMatrices_LessonPlans_LessonPlanId",
                table: "ExamMatrices");

            migrationBuilder.DropTable(
                name: "ExamMatrixDetails");

            migrationBuilder.RenameColumn(
                name: "TotalQuestions",
                table: "ExamMatrices",
                newName: "NumberOfQuestions");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ExamMatrices",
                newName: "Topic");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatrixId",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "ExamMatrices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarksPerQuestion",
                table: "ExamMatrices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExamMatrixLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsPerQuestion = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMatrixLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMatrixLines_ExamMatrices_MatrixId",
                        column: x => x.MatrixId,
                        principalTable: "ExamMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamMatrixLines_MatrixId",
                table: "ExamMatrixLines",
                column: "MatrixId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMatrices_LessonPlans_LessonPlanId",
                table: "ExamMatrices",
                column: "LessonPlanId",
                principalTable: "LessonPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
