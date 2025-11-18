using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class SubscriptionUsageLimits : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "CurrentLessonPlansCreated",
				table: "Subscriptions",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "CurrentPromptsUsed",
				table: "Subscriptions",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<DateTime>(
				name: "LastPromptResetDate",
				table: "Subscriptions",
				type: "datetime2",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "MaxLessonPlans",
				table: "SubscriptionPlans",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "MaxPromptsPerDay",
				table: "SubscriptionPlans",
				type: "int",
				nullable: false,
				defaultValue: 0);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "CurrentLessonPlansCreated",
				table: "Subscriptions");

			migrationBuilder.DropColumn(
				name: "CurrentPromptsUsed",
				table: "Subscriptions");

			migrationBuilder.DropColumn(
				name: "LastPromptResetDate",
				table: "Subscriptions");

			migrationBuilder.DropColumn(
				name: "MaxLessonPlans",
				table: "SubscriptionPlans");

			migrationBuilder.DropColumn(
				name: "MaxPromptsPerDay",
				table: "SubscriptionPlans");
		}
	}
}
