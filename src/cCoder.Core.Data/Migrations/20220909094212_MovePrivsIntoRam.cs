using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations
{
    public partial class MovePrivsIntoRam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "privilege_update");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "CMS",
                table: "Pages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "CMS",
                table: "Pages",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: true);

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "privilege_create", "Allows users to Create Privileges.", "Create", false, "Privilege" },
                    { "privilege_delete", "Allows users to Delete Privileges.", "Delete", false, "Privilege" },
                    { "privilege_read", "Allows users to Read Privileges.", "Read", false, "Privilege" },
                    { "privilege_update", "Allows users to Update Privileges.", "Update", false, "Privilege" }
                });
        }
    }
}
