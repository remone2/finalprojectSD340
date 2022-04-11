using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace finalprojectSD340.Data.Migrations
{
    public partial class CompleteNotificationSentBoolAddedToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CompleteNotificationSent",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompleteNotificationSent",
                table: "Tasks");
        }
    }
}
