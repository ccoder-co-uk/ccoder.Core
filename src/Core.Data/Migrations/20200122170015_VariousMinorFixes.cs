using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class VariousMinorFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppId",
                schema: "Mail",
                table: "SentEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SentByUserId",
                schema: "Mail",
                table: "SentEmails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppId",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SentByUserId",
                schema: "Mail",
                table: "QueuedEmails",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[] { "file_updatecontents", "Allows users to call UpdateContents on Files.", "UpdateContents", false, "File" });

            migrationBuilder.CreateIndex(
                name: "IX_SentEmails_AppId",
                schema: "Mail",
                table: "SentEmails",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmails_SentByUserId",
                schema: "Mail",
                table: "SentEmails",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEmails_AppId",
                schema: "Mail",
                table: "QueuedEmails",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEmails_SentByUserId",
                schema: "Mail",
                table: "QueuedEmails",
                column: "SentByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmails_Apps_AppId",
                schema: "Mail",
                table: "QueuedEmails",
                column: "AppId",
                principalSchema: "CMS",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmails_Users_SentByUserId",
                schema: "Mail",
                table: "QueuedEmails",
                column: "SentByUserId",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SentEmails_Apps_AppId",
                schema: "Mail",
                table: "SentEmails",
                column: "AppId",
                principalSchema: "CMS",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SentEmails_Users_SentByUserId",
                schema: "Mail",
                table: "SentEmails",
                column: "SentByUserId",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueuedEmails_Apps_AppId",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedEmails_Users_SentByUserId",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_SentEmails_Apps_AppId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_SentEmails_Users_SentByUserId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropIndex(
                name: "IX_SentEmails_AppId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropIndex(
                name: "IX_SentEmails_SentByUserId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropIndex(
                name: "IX_QueuedEmails_AppId",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropIndex(
                name: "IX_QueuedEmails_SentByUserId",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "file_updatecontents");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SentByUserId",
                schema: "Mail",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "Mail",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "SentByUserId",
                schema: "Mail",
                table: "QueuedEmails");
        }
    }
}