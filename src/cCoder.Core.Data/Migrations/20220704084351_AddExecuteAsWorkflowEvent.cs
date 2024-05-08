using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations;

public partial class AddExecuteAsWorkflowEvent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [Workflow].[WorkflowEvents]");
        migrationBuilder.AddColumn<string>(
            name: "ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowEvents_ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents",
            column: "ExecuteAs");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkflowEvents_Users_ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents",
            column: "ExecuteAs",
            principalSchema: "Security",
            principalTable: "Users",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkflowEvents_Users_ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowEvents_ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents");

        migrationBuilder.DropColumn(
            name: "ExecuteAs",
            schema: "Workflow",
            table: "WorkflowEvents");
    }
}
