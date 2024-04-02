using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class MakeExecuteAsForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTasks_ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                column: "ExecuteAs");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledTasks_Users_ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                column: "ExecuteAs",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledTasks_Users_ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledTasks_ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks");

            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Planning",
                table: "ScheduledTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
