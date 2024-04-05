using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace cCoder.Core.Migrations
{
    public partial class VariousNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_update");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Workflow",
                table: "WorkFlows",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "End",
                schema: "Workflow",
                table: "FlowInstances",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Start",
                schema: "Workflow",
                table: "FlowInstances",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Workflow",
                table: "BusinessProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Tasks",
                table: "ScheduledTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Planning",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Planning",
                table: "Calendars",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                schema: "Mail",
                table: "SentEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmptPass",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUser",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmptPass",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUser",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "DMS",
                table: "Files",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "DMS",
                table: "Files",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                schema: "DMS",
                table: "Files",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "DMS",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "DMS",
                table: "Files",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "DMS",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "DMS",
                table: "FileContents",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "DMS",
                table: "FileContents",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "DMS",
                table: "FileContents",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "page_updatecontents", "Allows users to call UpdateContents on Pages.", "UpdateContents", false, "Page" },
                    { "page_updateroles", "Allows users to call UpdateRoles on Pages.", "UpdateRoles", false, "Page" },
                    { "page_updateinfo", "Allows users to call UpdateInfo on Pages.", "UpdateInfo", false, "Page" },
                    { "folder_updatesubfolders", "Allows users to call UpdateSubFolders on Folders.", "UpdateSubFolders", false, "Folder" },
                    { "folder_updatefiles", "Allows users to call UpdateFiles on Folders.", "UpdateFiles", false, "Folder" },
                    { "folder_updateroles", "Allows users to call UpdateRoles on Folders.", "UpdateRoles", false, "Folder" },
                    { "calendarevent_create", "Allows users to Create CalendarEvents.", "Create", false, "CalendarEvent" },
                    { "calendarevent_read", "Allows users to Read CalendarEvents.", "Read", false, "CalendarEvent" },
                    { "calendarevent_update", "Allows users to Update CalendarEvents.", "Update", false, "CalendarEvent" },
                    { "calendarevent_delete", "Allows users to Delete CalendarEvents.", "Delete", false, "CalendarEvent" },
                    { "calendarevent_admin", "Allows users to Administer CalendarEvents.", "Admin", false, "CalendarEvent" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatefiles");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updateroles");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatesubfolders");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_updatecontents");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_updateinfo");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_updateroles");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Workflow",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "End",
                schema: "Workflow",
                table: "FlowInstances");

            migrationBuilder.DropColumn(
                name: "Start",
                schema: "Workflow",
                table: "FlowInstances");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Workflow",
                table: "BusinessProcesses");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Tasks",
                table: "ScheduledTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Planning",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Planning",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Port",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmptPass",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmtpUser",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "Port",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmptPass",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmtpUser",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "DMS",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "DMS",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "DMS",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "DMS",
                table: "FileContents");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "DMS",
                table: "FileContents");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "DMS",
                table: "FileContents");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "DMS",
                table: "Files",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "DMS",
                table: "Files",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                schema: "DMS",
                table: "Files",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "event_create", "Allows users to Create Events.", "Create", false, "Event" },
                    { "event_read", "Allows users to Read Events.", "Read", false, "Event" },
                    { "event_update", "Allows users to Update Events.", "Update", false, "Event" },
                    { "event_delete", "Allows users to Delete Events.", "Delete", false, "Event" },
                    { "event_admin", "Allows users to Administer Events.", "Admin", false, "Event" }
                });
        }
    }
}
