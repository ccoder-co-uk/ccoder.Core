using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class UpdateMailObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "From",
                schema: "Mail",
                table: "QueuedEmails",
                newName: "MailServerName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MailServerName",
                schema: "Mail",
                table: "QueuedEmails",
                newName: "From");
        }
    }
}