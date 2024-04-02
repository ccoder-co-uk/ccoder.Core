using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddMorePrivsToUsersAndGuests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Security].[Users] SET DefaultCultureId='en-GB' WHERE DefaultCultureId='en';
                UPDATE [Security].[Roles] SET [Privs]='culture_read,folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read' WHERE [Name]='Users';
                UPDATE [Security].[Roles] SET [Privs]='folderrole_read,pagerole_read,userrole_read,appculture_read,page_read,folder_read,file_read,app_read' WHERE [Name]='Guests';
            ");

            migrationBuilder.DeleteData(
                schema: "CMS",
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "DataJson",
                schema: "CMS",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "CMS",
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fr-FR",
                column: "Name",
                value: "French (France)");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "DataJson",
                schema: "CMS",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                schema: "CMS",
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fr-FR",
                column: "Name",
                value: "French");

            migrationBuilder.InsertData(
                schema: "CMS",
                table: "Cultures",
                columns: new[] { "Id", "Name" },
                values: new object[] { "en", "English" });


        }
    }
}
