using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class AddDescriptionToRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "Security",
            table: "Roles",
            nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Description",
            schema: "Security",
            table: "Roles");
}
