using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class UpdateBPtoWFKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessProcessWorkflows",
                schema: "Workflow");

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlows_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                column: "ProcessId",
                principalSchema: "Workflow",
                principalTable: "BusinessProcesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlows_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropIndex(
                name: "IX_WorkFlows_ProcessId",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.CreateTable(
                name: "BusinessProcessWorkflows",
                schema: "Workflow",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(nullable: false),
                    BusinessProcessId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProcessWorkflows", x => new { x.FlowId, x.BusinessProcessId });
                    table.ForeignKey(
                        name: "FK_BusinessProcessWorkflows_BusinessProcesses_BusinessProcessId",
                        column: x => x.BusinessProcessId,
                        principalSchema: "Workflow",
                        principalTable: "BusinessProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessProcessWorkflows_WorkFlows_FlowId",
                        column: x => x.FlowId,
                        principalSchema: "Workflow",
                        principalTable: "WorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProcessWorkflows_BusinessProcessId",
                schema: "Workflow",
                table: "BusinessProcessWorkflows",
                column: "BusinessProcessId");
        }
    }
}
