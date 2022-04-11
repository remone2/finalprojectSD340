using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace finalprojectSD340.Data.Migrations
{
    public partial class AddNotificationBoolsToTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeadlineNotificationSent",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineNotificationSent",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");
        }
    }
}
