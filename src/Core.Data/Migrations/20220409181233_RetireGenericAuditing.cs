using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Core.Migrations
{
    public partial class RetireGenericAuditing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditDataItems",
                schema: "Audit");

            migrationBuilder.DropTable(
                name: "AuditEntries",
                schema: "Audit");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_update");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Audit");

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                schema: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeOfEvent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditDataItems",
                schema: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditEntryId = table.Column<int>(type: "int", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditDataItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditDataItems_AuditEntries_AuditEntryId",
                        column: x => x.AuditEntryId,
                        principalSchema: "Audit",
                        principalTable: "AuditEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "auditdataitem_create", "Allows users to Create AuditDataItems.", "Create", false, "AuditDataItem" },
                    { "auditdataitem_delete", "Allows users to Delete AuditDataItems.", "Delete", false, "AuditDataItem" },
                    { "auditdataitem_read", "Allows users to Read AuditDataItems.", "Read", false, "AuditDataItem" },
                    { "auditdataitem_update", "Allows users to Update AuditDataItems.", "Update", false, "AuditDataItem" },
                    { "auditentry_create", "Allows users to Create AuditEntrys.", "Create", false, "AuditEntry" },
                    { "auditentry_delete", "Allows users to Delete AuditEntrys.", "Delete", false, "AuditEntry" },
                    { "auditentry_read", "Allows users to Read AuditEntrys.", "Read", false, "AuditEntry" },
                    { "auditentry_update", "Allows users to Update AuditEntrys.", "Update", false, "AuditEntry" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditDataItems_AuditEntryId",
                schema: "Audit",
                table: "AuditDataItems",
                column: "AuditEntryId");
        }
    }
}
