using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddRequiredAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                                    UPDATE WorkFlow.Workflows SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE WorkFlow.BusinessProcesses SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Templates SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Components SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Layouts SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Resources SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Pages SET CreatedOn=SYSUTCDATETIME() WHERE CreatedBy IS NULL;     
                                    UPDATE WorkFlow.Workflows SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE WorkFlow.BusinessProcesses SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Templates SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Components SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Layouts SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Resources SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Pages SET LastUpdated=CreatedOn WHERE LastUpdatedBy IS NULL;

                                    UPDATE WorkFlow.Workflows SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE WorkFlow.BusinessProcesses SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Templates SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Components SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Layouts SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Resources SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;
                                    UPDATE CMS.Pages SET CreatedBy='callum.marshall' WHERE CreatedBy IS NULL;     
                                    UPDATE WorkFlow.Workflows SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE WorkFlow.BusinessProcesses SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Templates SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Components SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Layouts SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Resources SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL;
                                    UPDATE CMS.Pages SET LastUpdatedBy=CreatedBy WHERE LastUpdatedBy IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Templates",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Resources",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Pages",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Layouts",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Components",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);


            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);


            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Templates",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Resources",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Pages",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Layouts",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Components",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "WorkFlows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Workflow",
                table: "BusinessProcesses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Layouts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Layouts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Components",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Components",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);
        }
    }
}
