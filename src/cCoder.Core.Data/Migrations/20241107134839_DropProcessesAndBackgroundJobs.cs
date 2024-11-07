using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace cCoder.Core.Migrations
{
    /// <inheritdoc />
    public partial class DropProcessesAndBackgroundJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlows_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropTable(
                name: "BackgroundJobs",
                schema: "Planning");

            migrationBuilder.DropTable(
                name: "BusinessProcesses",
                schema: "Workflow");

            migrationBuilder.DropIndex(
                name: "IX_WorkFlows_ProcessId",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "backgroundjob_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "backgroundjob_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "backgroundjob_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "backgroundjob_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_update");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkFlows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BackgroundJobs",
                schema: "Planning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    JobJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackgroundJobs_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProcesses",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReportingComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProcesses_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "backgroundjob_create", "Allows users to Create BackgroundJobs.", "Create", false, "BackgroundJob" },
                    { "backgroundjob_delete", "Allows users to Delete BackgroundJobs.", "Delete", false, "BackgroundJob" },
                    { "backgroundjob_read", "Allows users to Read BackgroundJobs.", "Read", false, "BackgroundJob" },
                    { "backgroundjob_update", "Allows users to Update BackgroundJobs.", "Update", false, "BackgroundJob" },
                    { "businessprocess_create", "Allows users to Create BusinessProcesses.", "Create", false, "BusinessProcess" },
                    { "businessprocess_delete", "Allows users to Delete BusinessProcesses.", "Delete", false, "BusinessProcess" },
                    { "businessprocess_read", "Allows users to Read BusinessProcesses.", "Read", false, "BusinessProcess" },
                    { "businessprocess_update", "Allows users to Update BusinessProcesses.", "Update", false, "BusinessProcess" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_AppId",
                schema: "Planning",
                table: "BackgroundJobs",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProcesses_AppId",
                schema: "Workflow",
                table: "BusinessProcesses",
                column: "AppId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlows_BusinessProcesses_ProcessId",
                schema: "Workflow",
                table: "WorkFlows",
                column: "ProcessId",
                principalSchema: "Workflow",
                principalTable: "BusinessProcesses",
                principalColumn: "Id");
        }
    }
}
