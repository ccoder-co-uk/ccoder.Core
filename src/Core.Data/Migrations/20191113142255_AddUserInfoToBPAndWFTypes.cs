using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class AddUserInfoToBPAndWFTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdated",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caller",
                schema: "Workflow",
                table: "FlowInstances",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                schema: "Workflow",
                table: "BusinessProcesses",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdated",
                schema: "Workflow",
                table: "BusinessProcesses",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "Caller",
                schema: "Workflow",
                table: "FlowInstances");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "Workflow",
                table: "BusinessProcesses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Workflow",
                table: "BusinessProcesses");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                schema: "Workflow",
                table: "BusinessProcesses");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "BusinessProcesses");
        }
    }
}
