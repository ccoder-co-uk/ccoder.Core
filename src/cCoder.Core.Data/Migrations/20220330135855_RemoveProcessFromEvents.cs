using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace cCoder.Core.Migrations
{
    public partial class RemoveProcessFromEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowEvents_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowEvents_ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_executeas");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_resume");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[] { "flowdefinition_executeas", "Allows users to call ExecuteAs on FlowDefinitions.", "ExecuteAs", false, "FlowDefinition" });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[] { "flowdefinition_resume", "Allows users to call Resume on FlowDefinitions.", "Resume", false, "FlowDefinition" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEvents_ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowEvents_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "ProcessId",
                principalSchema: "Workflow",
                principalTable: "BusinessProcesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
