using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfUrlToSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "Syllabuses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "Syllabuses");
        }
    }
}
