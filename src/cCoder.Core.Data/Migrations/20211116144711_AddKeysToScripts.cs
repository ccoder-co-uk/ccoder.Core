using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class AddKeysToScripts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                schema: "CMS",
                table: "Scripts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                schema: "CMS",
                table: "Scripts");
        }
    }
}
