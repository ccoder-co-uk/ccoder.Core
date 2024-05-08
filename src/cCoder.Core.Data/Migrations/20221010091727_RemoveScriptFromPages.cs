using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations;

public partial class RemoveScriptFromPages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Script",
            schema: "CMS",
            table: "Contents");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "Script",
            schema: "CMS",
            table: "Contents",
            type: "nvarchar(max)",
            nullable: true);
}
