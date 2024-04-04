using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class AddJoinEntityDBSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "appculture_create", "Allows users to Create AppCultures.", "Create", false, "AppCulture" },
                    { "userrole_update", "Allows users to Update UserRoles.", "Update", false, "UserRole" },
                    { "userrole_read", "Allows users to Read UserRoles.", "Read", false, "UserRole" },
                    { "userrole_create", "Allows users to Create UserRoles.", "Create", false, "UserRole" },
                    { "pagerole_admin", "Allows users to Administer PageRoles.", "Admin", false, "PageRole" },
                    { "pagerole_delete", "Allows users to Delete PageRoles.", "Delete", false, "PageRole" },
                    { "pagerole_update", "Allows users to Update PageRoles.", "Update", false, "PageRole" },
                    { "pagerole_read", "Allows users to Read PageRoles.", "Read", false, "PageRole" },
                    { "pagerole_create", "Allows users to Create PageRoles.", "Create", false, "PageRole" },
                    { "folderrole_admin", "Allows users to Administer FolderRoles.", "Admin", false, "FolderRole" },
                    { "folderrole_delete", "Allows users to Delete FolderRoles.", "Delete", false, "FolderRole" },
                    { "folderrole_update", "Allows users to Update FolderRoles.", "Update", false, "FolderRole" },
                    { "folderrole_read", "Allows users to Read FolderRoles.", "Read", false, "FolderRole" },
                    { "folderrole_create", "Allows users to Create FolderRoles.", "Create", false, "FolderRole" },
                    { "appculture_admin", "Allows users to Administer AppCultures.", "Admin", false, "AppCulture" },
                    { "appculture_delete", "Allows users to Delete AppCultures.", "Delete", false, "AppCulture" },
                    { "appculture_update", "Allows users to Update AppCultures.", "Update", false, "AppCulture" },
                    { "appculture_read", "Allows users to Read AppCultures.", "Read", false, "AppCulture" },
                    { "userrole_delete", "Allows users to Delete UserRoles.", "Delete", false, "UserRole" },
                    { "userrole_admin", "Allows users to Administer UserRoles.", "Admin", false, "UserRole" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "appculture_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folderrole_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "pagerole_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "userrole_update");
        }
    }
}
