using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class AddSystemUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "CMS",
                table: "CommonObjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.InsertData(
                 schema: "Security",
                 table: "Users",
                 columns: new[] { "Id", "DefaultCultureId", "DisplayName", "Email", "IsActive" },
                 values: new object[,]
                 {
                    { "system", "", "System", "system@corporatelinx.com", true }
                 }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "CMS",
                table: "CommonObjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
