using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace cCoder.Core.Migrations
{
    public partial class AddAuditFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowEvents_Users_ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkflowEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "Workflow",
                table: "WorkflowEvents",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowEvents_Users_ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "ExecuteAs",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowEvents_Users_ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "Workflow",
                table: "WorkflowEvents");

            migrationBuilder.AlterColumn<string>(
                name: "ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowEvents_Users_ExecuteAs",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "ExecuteAs",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
