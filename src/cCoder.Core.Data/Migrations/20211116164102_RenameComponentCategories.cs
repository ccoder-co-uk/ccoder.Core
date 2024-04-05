using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class RenameComponentCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                schema: "CMS",
                table: "Components",
                newName: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "CMS",
                table: "Components",
                newName: "Category");
        }
    }
}
