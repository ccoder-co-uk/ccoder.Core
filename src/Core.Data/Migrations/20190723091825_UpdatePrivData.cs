using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class UpdatePrivData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_getcontent");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_getconfig");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_getflow");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_unpack");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_computepermissions");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_contentforculture");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_infoforculture");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_setcontent");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_torenderresult");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_isadminofapp");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_isuserofapp");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Apps.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_create",
                column: "Description",
                value: "Allows users to Create Apps.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_delete",
                column: "Description",
                value: "Allows users to Delete Apps.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_read",
                column: "Description",
                value: "Allows users to Read Apps.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_update",
                column: "Description",
                value: "Allows users to Update Apps.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer AuditDataItems.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_create",
                column: "Description",
                value: "Allows users to Create AuditDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_delete",
                column: "Description",
                value: "Allows users to Delete AuditDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_read",
                column: "Description",
                value: "Allows users to Read AuditDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_update",
                column: "Description",
                value: "Allows users to Update AuditDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer AuditEntrys.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_create",
                column: "Description",
                value: "Allows users to Create AuditEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_delete",
                column: "Description",
                value: "Allows users to Delete AuditEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_read",
                column: "Description",
                value: "Allows users to Read AuditEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_update",
                column: "Description",
                value: "Allows users to Update AuditEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer BusinessProcesses.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_create",
                column: "Description",
                value: "Allows users to Create BusinessProcesses.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_delete",
                column: "Description",
                value: "Allows users to Delete BusinessProcesses.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_read",
                column: "Description",
                value: "Allows users to Read BusinessProcesses.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_update",
                column: "Description",
                value: "Allows users to Update BusinessProcesses.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Calendars.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_create",
                column: "Description",
                value: "Allows users to Create Calendars.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_delete",
                column: "Description",
                value: "Allows users to Delete Calendars.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_read",
                column: "Description",
                value: "Allows users to Read Calendars.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_update",
                column: "Description",
                value: "Allows users to Update Calendars.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Components.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_create",
                column: "Description",
                value: "Allows users to Create Components.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_delete",
                column: "Description",
                value: "Allows users to Delete Components.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_read",
                column: "Description",
                value: "Allows users to Read Components.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_render",
                column: "Description",
                value: "Allows users to call Render on Components.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_update",
                column: "Description",
                value: "Allows users to Update Components.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Contents.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_create",
                column: "Description",
                value: "Allows users to Create Contents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_delete",
                column: "Description",
                value: "Allows users to Delete Contents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_read",
                column: "Description",
                value: "Allows users to Read Contents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_update",
                column: "Description",
                value: "Allows users to Update Contents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Cultures.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_create",
                column: "Description",
                value: "Allows users to Create Cultures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_delete",
                column: "Description",
                value: "Allows users to Delete Cultures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_read",
                column: "Description",
                value: "Allows users to Read Cultures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_update",
                column: "Description",
                value: "Allows users to Update Cultures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer EmailSendFailures.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_create",
                column: "Description",
                value: "Allows users to Create EmailSendFailures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_delete",
                column: "Description",
                value: "Allows users to Delete EmailSendFailures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_read",
                column: "Description",
                value: "Allows users to Read EmailSendFailures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_update",
                column: "Description",
                value: "Allows users to Update EmailSendFailures.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Events.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_create",
                column: "Description",
                value: "Allows users to Create Events.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_delete",
                column: "Description",
                value: "Allows users to Delete Events.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_read",
                column: "Description",
                value: "Allows users to Read Events.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_update",
                column: "Description",
                value: "Allows users to Update Events.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Files.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_create",
                column: "Description",
                value: "Allows users to Create Files.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_delete",
                column: "Description",
                value: "Allows users to Delete Files.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_read",
                column: "Description",
                value: "Allows users to Read Files.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_update",
                column: "Description",
                value: "Allows users to Update Files.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FileContents.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_create",
                column: "Description",
                value: "Allows users to Create FileContents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_delete",
                column: "Description",
                value: "Allows users to Delete FileContents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_read",
                column: "Description",
                value: "Allows users to Read FileContents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_update",
                column: "Description",
                value: "Allows users to Update FileContents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FlowDefinitions.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_create",
                column: "Description",
                value: "Allows users to Create FlowDefinitions.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_delete",
                column: "Description",
                value: "Allows users to Delete FlowDefinitions.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_read",
                column: "Description",
                value: "Allows users to Read FlowDefinitions.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_update",
                column: "Description",
                value: "Allows users to Update FlowDefinitions.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FlowInstanceDatas.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_create",
                column: "Description",
                value: "Allows users to Create FlowInstanceDatas.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_delete",
                column: "Description",
                value: "Allows users to Delete FlowInstanceDatas.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_read",
                column: "Description",
                value: "Allows users to Read FlowInstanceDatas.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_update",
                column: "Description",
                value: "Allows users to Update FlowInstanceDatas.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Folders.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_create",
                column: "Description",
                value: "Allows users to Create Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_delete",
                column: "Description",
                value: "Allows users to Delete Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_read",
                column: "Description",
                value: "Allows users to Read Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_update",
                column: "Description",
                value: "Allows users to Update Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Forms.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_create",
                column: "Description",
                value: "Allows users to Create Forms.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_delete",
                column: "Description",
                value: "Allows users to Delete Forms.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_read",
                column: "Description",
                value: "Allows users to Read Forms.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_render",
                column: "Description",
                value: "Allows users to call Render on Forms.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_update",
                column: "Description",
                value: "Allows users to Update Forms.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Layouts.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_create",
                column: "Description",
                value: "Allows users to Create Layouts.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_delete",
                column: "Description",
                value: "Allows users to Delete Layouts.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_read",
                column: "Description",
                value: "Allows users to Read Layouts.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_update",
                column: "Description",
                value: "Allows users to Update Layouts.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer LogDataItems.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_create",
                column: "Description",
                value: "Allows users to Create LogDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_delete",
                column: "Description",
                value: "Allows users to Delete LogDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_read",
                column: "Description",
                value: "Allows users to Read LogDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_update",
                column: "Description",
                value: "Allows users to Update LogDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer LogEntrys.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_create",
                column: "Description",
                value: "Allows users to Create LogEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_delete",
                column: "Description",
                value: "Allows users to Delete LogEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_read",
                column: "Description",
                value: "Allows users to Read LogEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_update",
                column: "Description",
                value: "Allows users to Update LogEntrys.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer MetaItems.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_create",
                column: "Description",
                value: "Allows users to Create MetaItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_delete",
                column: "Description",
                value: "Allows users to Delete MetaItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_read",
                column: "Description",
                value: "Allows users to Read MetaItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_update",
                column: "Description",
                value: "Allows users to Update MetaItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Packages.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_create",
                column: "Description",
                value: "Allows users to Create Packages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_delete",
                column: "Description",
                value: "Allows users to Delete Packages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_read",
                column: "Description",
                value: "Allows users to Read Packages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_update",
                column: "Description",
                value: "Allows users to Update Packages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer PackageItems.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_create",
                column: "Description",
                value: "Allows users to Create PackageItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_delete",
                column: "Description",
                value: "Allows users to Delete PackageItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_read",
                column: "Description",
                value: "Allows users to Read PackageItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_update",
                column: "Description",
                value: "Allows users to Update PackageItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Pages.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_create",
                column: "Description",
                value: "Allows users to Create Pages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_delete",
                column: "Description",
                value: "Allows users to Delete Pages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_read",
                column: "Description",
                value: "Allows users to Read Pages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_update",
                column: "Description",
                value: "Allows users to Update Pages.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer PageInfos.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_create",
                column: "Description",
                value: "Allows users to Create PageInfos.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_delete",
                column: "Description",
                value: "Allows users to Delete PageInfos.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_read",
                column: "Description",
                value: "Allows users to Read PageInfos.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_update",
                column: "Description",
                value: "Allows users to Update PageInfos.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Privileges.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_create",
                column: "Description",
                value: "Allows users to Create Privileges.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_delete",
                column: "Description",
                value: "Allows users to Delete Privileges.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_read",
                column: "Description",
                value: "Allows users to Read Privileges.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_update",
                column: "Description",
                value: "Allows users to Update Privileges.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer QueuedEmails.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_create",
                column: "Description",
                value: "Allows users to Create QueuedEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_delete",
                column: "Description",
                value: "Allows users to Delete QueuedEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_read",
                column: "Description",
                value: "Allows users to Read QueuedEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_update",
                column: "Description",
                value: "Allows users to Update QueuedEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Resources.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_create",
                column: "Description",
                value: "Allows users to Create Resources.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_delete",
                column: "Description",
                value: "Allows users to Delete Resources.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_read",
                column: "Description",
                value: "Allows users to Read Resources.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_update",
                column: "Description",
                value: "Allows users to Update Resources.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Roles.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_create",
                column: "Description",
                value: "Allows users to Create Roles.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_delete",
                column: "Description",
                value: "Allows users to Delete Roles.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_read",
                column: "Description",
                value: "Allows users to Read Roles.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_update",
                column: "Description",
                value: "Allows users to Update Roles.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Schedules.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_create",
                column: "Description",
                value: "Allows users to Create Schedules.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_delete",
                column: "Description",
                value: "Allows users to Delete Schedules.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_read",
                column: "Description",
                value: "Allows users to Read Schedules.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_update",
                column: "Description",
                value: "Allows users to Update Schedules.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer ScheduledTasks.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_create",
                column: "Description",
                value: "Allows users to Create ScheduledTasks.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_delete",
                column: "Description",
                value: "Allows users to Delete ScheduledTasks.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_read",
                column: "Description",
                value: "Allows users to Read ScheduledTasks.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_update",
                column: "Description",
                value: "Allows users to Update ScheduledTasks.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer ScheduledTaskDataItems.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_create",
                column: "Description",
                value: "Allows users to Create ScheduledTaskDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_delete",
                column: "Description",
                value: "Allows users to Delete ScheduledTaskDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_read",
                column: "Description",
                value: "Allows users to Read ScheduledTaskDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_update",
                column: "Description",
                value: "Allows users to Update ScheduledTaskDataItems.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer SentEmails.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_create",
                column: "Description",
                value: "Allows users to Create SentEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_delete",
                column: "Description",
                value: "Allows users to Delete SentEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_read",
                column: "Description",
                value: "Allows users to Read SentEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_update",
                column: "Description",
                value: "Allows users to Update SentEmails.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Templates.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_buildemailto",
                column: "Description",
                value: "Allows users to call BuildEmailTo on Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_create",
                column: "Description",
                value: "Allows users to Create Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_delete",
                column: "Description",
                value: "Allows users to Delete Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_read",
                column: "Description",
                value: "Allows users to Read Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_render",
                column: "Description",
                value: "Allows users to call Render on Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_update",
                column: "Description",
                value: "Allows users to Update Templates.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Users.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_create",
                column: "Description",
                value: "Allows users to Create Users.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_delete",
                column: "Description",
                value: "Allows users to Delete Users.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_read",
                column: "Description",
                value: "Allows users to Read Users.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_update",
                column: "Description",
                value: "Allows users to Update Users.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer WorkflowEvents.", "Admin" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_create",
                column: "Description",
                value: "Allows users to Create WorkflowEvents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_delete",
                column: "Description",
                value: "Allows users to Delete WorkflowEvents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_read",
                column: "Description",
                value: "Allows users to Read WorkflowEvents.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_update",
                column: "Description",
                value: "Allows users to Update WorkflowEvents.");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer App objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_create",
                column: "Description",
                value: "Allows users to Create App objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_delete",
                column: "Description",
                value: "Allows users to Delete App objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_read",
                column: "Description",
                value: "Allows users to Read App objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "app_update",
                column: "Description",
                value: "Allows users to Update App objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer AuditDataItem objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_create",
                column: "Description",
                value: "Allows users to Create AuditDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_delete",
                column: "Description",
                value: "Allows users to Delete AuditDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_read",
                column: "Description",
                value: "Allows users to Read AuditDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditdataitem_update",
                column: "Description",
                value: "Allows users to Update AuditDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer AuditEntry objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_create",
                column: "Description",
                value: "Allows users to Create AuditEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_delete",
                column: "Description",
                value: "Allows users to Delete AuditEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_read",
                column: "Description",
                value: "Allows users to Read AuditEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "auditentry_update",
                column: "Description",
                value: "Allows users to Update AuditEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer BusinessProcess objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_create",
                column: "Description",
                value: "Allows users to Create BusinessProcess objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_delete",
                column: "Description",
                value: "Allows users to Delete BusinessProcess objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_read",
                column: "Description",
                value: "Allows users to Read BusinessProcess objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "businessprocess_update",
                column: "Description",
                value: "Allows users to Update BusinessProcess objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Calendar objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_create",
                column: "Description",
                value: "Allows users to Create Calendar objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_delete",
                column: "Description",
                value: "Allows users to Delete Calendar objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_read",
                column: "Description",
                value: "Allows users to Read Calendar objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "calendar_update",
                column: "Description",
                value: "Allows users to Update Calendar objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Component objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_create",
                column: "Description",
                value: "Allows users to Create Component objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_delete",
                column: "Description",
                value: "Allows users to Delete Component objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_read",
                column: "Description",
                value: "Allows users to Read Component objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_render",
                column: "Description",
                value: "Allows users to call Render on Component objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "component_update",
                column: "Description",
                value: "Allows users to Update Component objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Content objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_create",
                column: "Description",
                value: "Allows users to Create Content objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_delete",
                column: "Description",
                value: "Allows users to Delete Content objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_read",
                column: "Description",
                value: "Allows users to Read Content objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "content_update",
                column: "Description",
                value: "Allows users to Update Content objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Culture objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_create",
                column: "Description",
                value: "Allows users to Create Culture objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_delete",
                column: "Description",
                value: "Allows users to Delete Culture objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_read",
                column: "Description",
                value: "Allows users to Read Culture objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "culture_update",
                column: "Description",
                value: "Allows users to Update Culture objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer EmailSendFailure objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_create",
                column: "Description",
                value: "Allows users to Create EmailSendFailure objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_delete",
                column: "Description",
                value: "Allows users to Delete EmailSendFailure objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_read",
                column: "Description",
                value: "Allows users to Read EmailSendFailure objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "emailsendfailure_update",
                column: "Description",
                value: "Allows users to Update EmailSendFailure objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Event objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_create",
                column: "Description",
                value: "Allows users to Create Event objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_delete",
                column: "Description",
                value: "Allows users to Delete Event objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_read",
                column: "Description",
                value: "Allows users to Read Event objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "event_update",
                column: "Description",
                value: "Allows users to Update Event objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer File objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_create",
                column: "Description",
                value: "Allows users to Create File objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_delete",
                column: "Description",
                value: "Allows users to Delete File objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_read",
                column: "Description",
                value: "Allows users to Read File objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_update",
                column: "Description",
                value: "Allows users to Update File objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FileContent objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_create",
                column: "Description",
                value: "Allows users to Create FileContent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_delete",
                column: "Description",
                value: "Allows users to Delete FileContent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_read",
                column: "Description",
                value: "Allows users to Read FileContent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "filecontent_update",
                column: "Description",
                value: "Allows users to Update FileContent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FlowDefinition objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_create",
                column: "Description",
                value: "Allows users to Create FlowDefinition objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_delete",
                column: "Description",
                value: "Allows users to Delete FlowDefinition objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_read",
                column: "Description",
                value: "Allows users to Read FlowDefinition objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowdefinition_update",
                column: "Description",
                value: "Allows users to Update FlowDefinition objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer FlowInstanceData objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_create",
                column: "Description",
                value: "Allows users to Create FlowInstanceData objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_delete",
                column: "Description",
                value: "Allows users to Delete FlowInstanceData objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_read",
                column: "Description",
                value: "Allows users to Read FlowInstanceData objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "flowinstancedata_update",
                column: "Description",
                value: "Allows users to Update FlowInstanceData objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Folder objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_create",
                column: "Description",
                value: "Allows users to Create Folder objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_delete",
                column: "Description",
                value: "Allows users to Delete Folder objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_read",
                column: "Description",
                value: "Allows users to Read Folder objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_update",
                column: "Description",
                value: "Allows users to Update Folder objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Form objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_create",
                column: "Description",
                value: "Allows users to Create Form objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_delete",
                column: "Description",
                value: "Allows users to Delete Form objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_read",
                column: "Description",
                value: "Allows users to Read Form objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_render",
                column: "Description",
                value: "Allows users to call Render on Form objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_update",
                column: "Description",
                value: "Allows users to Update Form objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Layout objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_create",
                column: "Description",
                value: "Allows users to Create Layout objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_delete",
                column: "Description",
                value: "Allows users to Delete Layout objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_read",
                column: "Description",
                value: "Allows users to Read Layout objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "layout_update",
                column: "Description",
                value: "Allows users to Update Layout objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer LogDataItem objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_create",
                column: "Description",
                value: "Allows users to Create LogDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_delete",
                column: "Description",
                value: "Allows users to Delete LogDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_read",
                column: "Description",
                value: "Allows users to Read LogDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logdataitem_update",
                column: "Description",
                value: "Allows users to Update LogDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer LogEntry objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_create",
                column: "Description",
                value: "Allows users to Create LogEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_delete",
                column: "Description",
                value: "Allows users to Delete LogEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_read",
                column: "Description",
                value: "Allows users to Read LogEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "logentry_update",
                column: "Description",
                value: "Allows users to Update LogEntry objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer MetaItem objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_create",
                column: "Description",
                value: "Allows users to Create MetaItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_delete",
                column: "Description",
                value: "Allows users to Delete MetaItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_read",
                column: "Description",
                value: "Allows users to Read MetaItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_update",
                column: "Description",
                value: "Allows users to Update MetaItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Package objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_create",
                column: "Description",
                value: "Allows users to Create Package objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_delete",
                column: "Description",
                value: "Allows users to Delete Package objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_read",
                column: "Description",
                value: "Allows users to Read Package objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "package_update",
                column: "Description",
                value: "Allows users to Update Package objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer PackageItem objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_create",
                column: "Description",
                value: "Allows users to Create PackageItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_delete",
                column: "Description",
                value: "Allows users to Delete PackageItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_read",
                column: "Description",
                value: "Allows users to Read PackageItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "packageitem_update",
                column: "Description",
                value: "Allows users to Update PackageItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Page objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_create",
                column: "Description",
                value: "Allows users to Create Page objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_delete",
                column: "Description",
                value: "Allows users to Delete Page objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_read",
                column: "Description",
                value: "Allows users to Read Page objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "page_update",
                column: "Description",
                value: "Allows users to Update Page objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer PageInfo objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_create",
                column: "Description",
                value: "Allows users to Create PageInfo objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_delete",
                column: "Description",
                value: "Allows users to Delete PageInfo objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_read",
                column: "Description",
                value: "Allows users to Read PageInfo objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pageinfo_update",
                column: "Description",
                value: "Allows users to Update PageInfo objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Privilege objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_create",
                column: "Description",
                value: "Allows users to Create Privilege objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_delete",
                column: "Description",
                value: "Allows users to Delete Privilege objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_read",
                column: "Description",
                value: "Allows users to Read Privilege objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_update",
                column: "Description",
                value: "Allows users to Update Privilege objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer QueuedEmail objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_create",
                column: "Description",
                value: "Allows users to Create QueuedEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_delete",
                column: "Description",
                value: "Allows users to Delete QueuedEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_read",
                column: "Description",
                value: "Allows users to Read QueuedEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "queuedemail_update",
                column: "Description",
                value: "Allows users to Update QueuedEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Resource objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_create",
                column: "Description",
                value: "Allows users to Create Resource objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_delete",
                column: "Description",
                value: "Allows users to Delete Resource objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_read",
                column: "Description",
                value: "Allows users to Read Resource objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "resource_update",
                column: "Description",
                value: "Allows users to Update Resource objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Role objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_create",
                column: "Description",
                value: "Allows users to Create Role objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_delete",
                column: "Description",
                value: "Allows users to Delete Role objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_read",
                column: "Description",
                value: "Allows users to Read Role objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "role_update",
                column: "Description",
                value: "Allows users to Update Role objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Schedule objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_create",
                column: "Description",
                value: "Allows users to Create Schedule objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_delete",
                column: "Description",
                value: "Allows users to Delete Schedule objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_read",
                column: "Description",
                value: "Allows users to Read Schedule objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "schedule_update",
                column: "Description",
                value: "Allows users to Update Schedule objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer ScheduledTask objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_create",
                column: "Description",
                value: "Allows users to Create ScheduledTask objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_delete",
                column: "Description",
                value: "Allows users to Delete ScheduledTask objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_read",
                column: "Description",
                value: "Allows users to Read ScheduledTask objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtask_update",
                column: "Description",
                value: "Allows users to Update ScheduledTask objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer ScheduledTaskDataItem objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_create",
                column: "Description",
                value: "Allows users to Create ScheduledTaskDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_delete",
                column: "Description",
                value: "Allows users to Delete ScheduledTaskDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_read",
                column: "Description",
                value: "Allows users to Read ScheduledTaskDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "scheduledtaskdataitem_update",
                column: "Description",
                value: "Allows users to Update ScheduledTaskDataItem objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer SentEmail objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_create",
                column: "Description",
                value: "Allows users to Create SentEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_delete",
                column: "Description",
                value: "Allows users to Delete SentEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_read",
                column: "Description",
                value: "Allows users to Read SentEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "sentemail_update",
                column: "Description",
                value: "Allows users to Update SentEmail objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer Template objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_buildemailto",
                column: "Description",
                value: "Allows users to call BuildEmailTo on Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_create",
                column: "Description",
                value: "Allows users to Create Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_delete",
                column: "Description",
                value: "Allows users to Delete Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_read",
                column: "Description",
                value: "Allows users to Read Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_render",
                column: "Description",
                value: "Allows users to call Render on Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "template_update",
                column: "Description",
                value: "Allows users to Update Template objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer User objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_create",
                column: "Description",
                value: "Allows users to Create User objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_delete",
                column: "Description",
                value: "Allows users to Delete User objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_read",
                column: "Description",
                value: "Allows users to Read User objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "user_update",
                column: "Description",
                value: "Allows users to Update User objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_admin",
                columns: new[] { "Description", "Operation" },
                values: new object[] { "Allows users to Administer WorkflowEvent objects.", "Delete" });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_create",
                column: "Description",
                value: "Allows users to Create WorkflowEvent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_delete",
                column: "Description",
                value: "Allows users to Delete WorkflowEvent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_read",
                column: "Description",
                value: "Allows users to Read WorkflowEvent objects.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "workflowevent_update",
                column: "Description",
                value: "Allows users to Update WorkflowEvent objects.");

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "user_isadminofapp", "Allows users to call IsAdminOfApp on User objects.", "IsAdminOfApp", false, "User" },
                    { "page_computepermissions", "Allows users to call ComputePermissions on Page objects.", "ComputePermissions", false, "Page" },
                    { "page_torenderresult", "Allows users to call ToRenderResult on Page objects.", "ToRenderResult", false, "Page" },
                    { "page_contentforculture", "Allows users to call ContentForCulture on Page objects.", "ContentForCulture", false, "Page" },
                    { "page_infoforculture", "Allows users to call InfoForCulture on Page objects.", "InfoForCulture", false, "Page" },
                    { "page_setcontent", "Allows users to call SetContent on Page objects.", "SetContent", false, "Page" },
                    { "packageitem_unpack", "Allows users to call Unpack on PackageItem objects.", "Unpack", false, "PackageItem" },
                    { "flowdefinition_getconfig", "Allows users to call GetConfig on FlowDefinition objects.", "GetConfig", false, "FlowDefinition" },
                    { "flowdefinition_getflow", "Allows users to call GetFlow on FlowDefinition objects.", "GetFlow", false, "FlowDefinition" },
                    { "file_getcontent", "Allows users to call GetContent on File objects.", "GetContent", false, "File" },
                    { "user_isuserofapp", "Allows users to call IsUserOfApp on User objects.", "IsUserOfApp", false, "User" }
                });
        }
    }
}
