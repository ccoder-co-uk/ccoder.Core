using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddMailServers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Port",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmptPass",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SmtpUser",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "Port",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmptPass",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SmtpUser",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.AddColumn<bool>(
                name: "IsBodyHtml",
                schema: "Mail",
                table: "SentEmails",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBodyHtml",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MailServers",
                schema: "Mail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    User = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    Host = table.Column<string>(nullable: false),
                    Port = table.Column<int>(nullable: false),
                    EnableSSL = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailServers_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatefiles",
                column: "Description",
                value: "Allows users to manage Files for a folder.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updateroles",
                column: "Description",
                value: "Allows users to manage Roles for a folder.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatesubfolders",
                column: "Description",
                value: "Allows users to manage Sub folders for a folder.");

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "mailserver_create", "Allows users to Create MailServers.", "Create", false, "MailServer" },
                    { "mailserver_read", "Allows users to Read MailServers.", "Read", false, "MailServer" },
                    { "mailserver_update", "Allows users to Update MailServers.", "Update", false, "MailServer" },
                    { "mailserver_delete", "Allows users to Delete MailServers.", "Delete", false, "MailServer" },
                    { "mailserver_admin", "Allows users to Administer MailServers.", "Admin", false, "MailServer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailServers_AppId",
                schema: "Mail",
                table: "MailServers",
                column: "AppId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailServers",
                schema: "Mail");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_admin");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "mailserver_update");

            migrationBuilder.DropColumn(
                name: "IsBodyHtml",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "IsBodyHtml",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.AddColumn<int>(
                name: "Port",
                schema: "Mail",
                table: "SentEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmptPass",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUser",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmptPass",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUser",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatefiles",
                column: "Description",
                value: "Allows users to call UpdateFiles on Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updateroles",
                column: "Description",
                value: "Allows users to call UpdateRoles on Folders.");

            migrationBuilder.UpdateData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "folder_updatesubfolders",
                column: "Description",
                value: "Allows users to call UpdateSubFolders on Folders.");
        }
    }
}
