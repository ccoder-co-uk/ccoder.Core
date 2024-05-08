using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class AddBaseEntityProperties : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {


        migrationBuilder.RenameColumn("Created", "Workflows", "CreatedOn", "Workflow");
        migrationBuilder.RenameColumn("Created", "BusinessProcesses", "CreatedOn", "Workflow");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Workflow",
            table: "WorkFlows",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Workflow",
            table: "WorkFlows",
            maxLength: 350,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Workflow",
            table: "BusinessProcesses",
            maxLength: 350,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Templates",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "CMS",
            table: "Templates",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedOn",
            schema: "CMS",
            table: "Templates",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Templates",
            maxLength: 350,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "CMS",
            table: "Templates",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Templates",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Resources",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Resources",
            maxLength: 350,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "CMS",
            table: "Resources",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedOn",
            schema: "CMS",
            table: "Resources",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "CMS",
            table: "Resources",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Resources",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "CMS",
            table: "Pages",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedOn",
            schema: "CMS",
            table: "Pages",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Pages",
            maxLength: 350,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "CMS",
            table: "Pages",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Pages",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Pages",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Layouts",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "CMS",
            table: "Layouts",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedOn",
            schema: "CMS",
            table: "Layouts",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Layouts",
            maxLength: 350,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "CMS",
            table: "Layouts",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Layouts",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Components",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "CMS",
            table: "Components",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedOn",
            schema: "CMS",
            table: "Components",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Components",
            maxLength: 350,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "CMS",
            table: "Components",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Components",
            nullable: true);

        migrationBuilder.InsertData(
            schema: "Security",
            table: "Privileges",
            columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
            values: new object[] { "flowdefinition_executeas", "Allows users to call ExecuteAs on FlowDefinitions.", "ExecuteAs", false, "FlowDefinition" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "flowdefinition_executeas");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "Workflow",
            table: "WorkFlows");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "Workflow",
            table: "BusinessProcesses");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "CMS",
            table: "Templates");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "CMS",
            table: "Templates");

        migrationBuilder.DropColumn(
            name: "Description",
            schema: "CMS",
            table: "Templates");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "CMS",
            table: "Templates");

        migrationBuilder.DropColumn(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Templates");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "CMS",
            table: "Resources");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "CMS",
            table: "Resources");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "CMS",
            table: "Resources");

        migrationBuilder.DropColumn(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Resources");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "Description",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "Name",
            schema: "CMS",
            table: "Pages");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "CMS",
            table: "Layouts");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "CMS",
            table: "Layouts");

        migrationBuilder.DropColumn(
            name: "Description",
            schema: "CMS",
            table: "Layouts");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "CMS",
            table: "Layouts");

        migrationBuilder.DropColumn(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Layouts");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "CMS",
            table: "Components");

        migrationBuilder.DropColumn(
            name: "CreatedOn",
            schema: "CMS",
            table: "Components");

        migrationBuilder.DropColumn(
            name: "Description",
            schema: "CMS",
            table: "Components");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "CMS",
            table: "Components");

        migrationBuilder.DropColumn(
            name: "LastUpdatedBy",
            schema: "CMS",
            table: "Components");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Workflow",
            table: "WorkFlows",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Workflow",
            table: "WorkFlows",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldMaxLength: 350,
            oldNullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Created",
            schema: "Workflow",
            table: "WorkFlows",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Workflow",
            table: "BusinessProcesses",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldMaxLength: 350,
            oldNullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Created",
            schema: "Workflow",
            table: "BusinessProcesses",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Templates",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Resources",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "CMS",
            table: "Resources",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 350,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Layouts",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "CMS",
            table: "Components",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 100);
    }
}
