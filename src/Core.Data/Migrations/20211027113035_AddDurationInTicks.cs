using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class AddDurationInTicks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                schema: "Planning",
                table: "Events");

            migrationBuilder.AddColumn<long>(
                name: "DurationInTicks",
                schema: "Planning",
                table: "Events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInTicks",
                schema: "Planning",
                table: "Events");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                schema: "Planning",
                table: "Events",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
