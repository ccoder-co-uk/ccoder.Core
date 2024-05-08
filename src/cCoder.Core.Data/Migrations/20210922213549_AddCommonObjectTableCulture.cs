using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class AddCommonObjectTableCulture : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "Culture",
            schema: "CMS",
            table: "CommonObjects",
            type: "nvarchar(max)",
            nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Culture",
            schema: "CMS",
            table: "CommonObjects");
}
