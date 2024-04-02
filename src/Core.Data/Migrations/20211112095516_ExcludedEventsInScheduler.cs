using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class ExcludedEventsInScheduler : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExcludedEventsName",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTasks_ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks",
                column: "ExcludedEventsCalendarId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledTasks_Calendars_ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks",
                column: "ExcludedEventsCalendarId",
                principalSchema: "Planning",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledTasks_Calendars_ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledTasks_ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.DropColumn(
                name: "ExcludedEventsCalendarId",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.DropColumn(
                name: "ExcludedEventsName",
                schema: "Planning",
                table: "ScheduledTasks");
        }
    }
}
