using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class NewReportingFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InstanceReportingComponentName",
            schema: "Workflow",
            table: "WorkFlows",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "WorkFlows",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "FlowInstances",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "BusinessProcesses",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "InstanceReportingComponentName",
            schema: "Workflow",
            table: "WorkFlows");

        migrationBuilder.DropColumn(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "WorkFlows");

        migrationBuilder.DropColumn(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "FlowInstances");

        migrationBuilder.DropColumn(
            name: "ReportingComponentName",
            schema: "Workflow",
            table: "BusinessProcesses");
    }
}
