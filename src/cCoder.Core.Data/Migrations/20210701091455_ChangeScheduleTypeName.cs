using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace cCoder.Core.Migrations
{
    public partial class ChangeScheduleTypeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Schedule",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.AddColumn<long>(
                name: "ScheduleInTicks",
                schema: "Planning",
                table: "ScheduledTasks",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("UPDATE [Planning].[ScheduledTasks] SET ScheduleInTicks=864000000000");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleInTicks",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Schedule",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "time",
                nullable: true);

            migrationBuilder.Sql("UPDATE [Planning].[ScheduledTasks] SET Schedule='23:59:00'");

        }
    }
}
