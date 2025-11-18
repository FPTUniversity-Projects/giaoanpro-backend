using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateLessonPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BankId",
                table: "Questions",
                newName: "LessonPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LessonPlanId",
                table: "Questions",
                column: "LessonPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_LessonPlans_LessonPlanId",
                table: "Questions",
                column: "LessonPlanId",
                principalTable: "LessonPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_LessonPlans_LessonPlanId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_LessonPlanId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "LessonPlanId",
                table: "Questions",
                newName: "BankId");
        }
    }
}
