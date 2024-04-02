using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class AddFlowInstances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowInstances",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FlowDefinitionId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ContextJson = table.Column<byte[]>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    ReportingComponentName = table.Column<string>(nullable: true),
                    Caller = table.Column<string>(nullable: true),
                    Start = table.Column<DateTimeOffset>(nullable: false),
                    End = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowInstances_WorkFlows_FlowDefinitionId",
                        column: x => x.FlowDefinitionId,
                        principalSchema: "Workflow",
                        principalTable: "WorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "flowinstancedata_create", "Allows users to Create FlowInstanceDatas.", "Create", false, "FlowInstanceData" },
                    { "flowinstancedata_read", "Allows users to Read FlowInstanceDatas.", "Read", false, "FlowInstanceData" },
                    { "flowinstancedata_update", "Allows users to Update FlowInstanceDatas.", "Update", false, "FlowInstanceData" },
                    { "flowinstancedata_delete", "Allows users to Delete FlowInstanceDatas.", "Delete", false, "FlowInstanceData" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowInstances_FlowDefinitionId",
                schema: "Workflow",
                table: "FlowInstances",
                column: "FlowDefinitionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowInstances",
                schema: "Workflow");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_update");
        }
    }
}
