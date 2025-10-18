using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddIsActivityIntoSubscriptionPlan : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "IsActive",
				table: "SubscriptionPlans",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.CreateTable(
				name: "Payments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
					PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					Status = table.Column<int>(type: "int", nullable: false),
					PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
					GatewayTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Payments", x => x.Id);
					table.ForeignKey(
						name: "FK_Payments_Subscriptions_SubscriptionId",
						column: x => x.SubscriptionId,
						principalTable: "Subscriptions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Payments_SubscriptionId",
				table: "Payments",
				column: "SubscriptionId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Payments");

			migrationBuilder.DropColumn(
				name: "IsActive",
				table: "SubscriptionPlans");
		}
	}
}
