using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class RemoveOldAdminPrivs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendarevent_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_admin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "layout_admin", "Allows users to Administer Layouts.", "Admin", false, "Layout" },
                    { "businessprocess_admin", "Allows users to Administer BusinessProcesses.", "Admin", false, "BusinessProcess" },
                    { "workflowevent_admin", "Allows users to Administer WorkflowEvents.", "Admin", false, "WorkflowEvent" },
                    { "flowdefinition_admin", "Allows users to Administer FlowDefinitions.", "Admin", false, "FlowDefinition" },
                    { "flowinstancedata_admin", "Allows users to Administer FlowInstanceDatas.", "Admin", false, "FlowInstanceData" },
                    { "package_admin", "Allows users to Administer Packages.", "Admin", false, "Package" },
                    { "packageitem_admin", "Allows users to Administer PackageItems.", "Admin", false, "PackageItem" },
                    { "role_admin", "Allows users to Administer Roles.", "Admin", false, "Role" },
                    { "privilege_admin", "Allows users to Administer Privileges.", "Admin", false, "Privilege" },
                    { "appculture_admin", "Allows users to Administer AppCultures.", "Admin", false, "AppCulture" },
                    { "folderrole_admin", "Allows users to Administer FolderRoles.", "Admin", false, "FolderRole" },
                    { "pagerole_admin", "Allows users to Administer PageRoles.", "Admin", false, "PageRole" },
                    { "userrole_admin", "Allows users to Administer UserRoles.", "Admin", false, "UserRole" },
                    { "auditentry_admin", "Allows users to Administer AuditEntrys.", "Admin", false, "AuditEntry" },
                    { "auditdataitem_admin", "Allows users to Administer AuditDataItems.", "Admin", false, "AuditDataItem" },
                    { "logentry_admin", "Allows users to Administer LogEntrys.", "Admin", false, "LogEntry" },
                    { "emailsendfailure_admin", "Allows users to Administer EmailSendFailures.", "Admin", false, "EmailSendFailure" },
                    { "sentemail_admin", "Allows users to Administer SentEmails.", "Admin", false, "SentEmail" },
                    { "queuedemail_admin", "Allows users to Administer QueuedEmails.", "Admin", false, "QueuedEmail" },
                    { "mailserver_admin", "Allows users to Administer MailServers.", "Admin", false, "MailServer" },
                    { "app_admin", "Allows users to Administer Apps.", "Admin", false, "App" },
                    { "page_admin", "Allows users to Administer Pages.", "Admin", false, "Page" },
                    { "pageinfo_admin", "Allows users to Administer PageInfos.", "Admin", false, "PageInfo" },
                    { "content_admin", "Allows users to Administer Contents.", "Admin", false, "Content" },
                    { "component_admin", "Allows users to Administer Components.", "Admin", false, "Component" },
                    { "resource_admin", "Allows users to Administer Resources.", "Admin", false, "Resource" },
                    { "culture_admin", "Allows users to Administer Cultures.", "Admin", false, "Culture" },
                    { "logdataitem_admin", "Allows users to Administer LogDataItems.", "Admin", false, "LogDataItem" },
                    { "form_admin", "Allows users to Administer Forms.", "Admin", false, "Form" },
                    { "metaitem_admin", "Allows users to Administer MetaItems.", "Admin", false, "MetaItem" },
                    { "folder_admin", "Allows users to Administer Folders.", "Admin", false, "Folder" },
                    { "file_admin", "Allows users to Administer Files.", "Admin", false, "File" },
                    { "filecontent_admin", "Allows users to Administer FileContents.", "Admin", false, "FileContent" },
                    { "calendar_admin", "Allows users to Administer Calendars.", "Admin", false, "Calendar" },
                    { "calendarevent_admin", "Allows users to Administer CalendarEvents.", "Admin", false, "CalendarEvent" },
                    { "scheduledtask_admin", "Allows users to Administer ScheduledTasks.", "Admin", false, "ScheduledTask" },
                    { "template_admin", "Allows users to Administer Templates.", "Admin", false, "Template" },
                    { "user_admin", "Allows users to Administer Users.", "Admin", false, "User" }
                });
        }
    }
}
