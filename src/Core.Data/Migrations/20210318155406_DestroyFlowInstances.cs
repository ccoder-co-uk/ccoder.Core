using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace cCoder.Core.Migrations
{
    public partial class DestroyFlowInstances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowInstances",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Caller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    End = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FlowDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportingComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Start = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
    }
}
