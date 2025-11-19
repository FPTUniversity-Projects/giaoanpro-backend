using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class RefreshToken : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "RefreshToken",
				table: "Users",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<DateTime>(
				name: "RefreshTokenExpiryTime",
				table: "Users",
				type: "datetime2",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "RefreshToken",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "RefreshTokenExpiryTime",
				table: "Users");
		}
	}
}
