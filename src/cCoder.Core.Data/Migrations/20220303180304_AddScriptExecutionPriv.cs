using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations;

public partial class AddScriptExecutionPriv : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
INSERT INTO [Security].[Privileges] ([Id], [Type], [Operation], [Description], [PortalAdminsOnly])
VALUES ('script_execute', 'FlowDefinition', 'ExecuteScript', 'Allows users to execute anarbitrary block of C# on the workflow engine', 0)
            ");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
DELETE [Security].[Privileges] WHERE Id = 'script_execute'
            ");
}