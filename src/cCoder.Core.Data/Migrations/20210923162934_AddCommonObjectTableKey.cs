using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class AddCommonObjectTableKey : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "Key",
            schema: "CMS",
            table: "CommonObjects",
            type: "nvarchar(max)",
            nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Key",
            schema: "CMS",
            table: "CommonObjects");
}
