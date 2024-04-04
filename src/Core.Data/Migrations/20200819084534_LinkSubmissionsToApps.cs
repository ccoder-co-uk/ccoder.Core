using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations
{
    public partial class LinkSubmissionsToApps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppId",
                schema: "CMS",
                table: "Submissions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AppId",
                schema: "CMS",
                table: "Submissions",
                column: "AppId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Apps_AppId",
                schema: "CMS",
                table: "Submissions",
                column: "AppId",
                principalSchema: "CMS",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Apps_AppId",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_AppId",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "CMS",
                table: "Submissions");
        }
    }
}
