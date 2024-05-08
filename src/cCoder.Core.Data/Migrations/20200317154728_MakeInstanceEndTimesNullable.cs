using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class MakeInstanceEndTimesNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "End",
            schema: "Workflow",
            table: "FlowInstances",
            nullable: true,
            oldClrType: typeof(DateTimeOffset));

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "End",
            schema: "Workflow",
            table: "FlowInstances",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldNullable: true);
}