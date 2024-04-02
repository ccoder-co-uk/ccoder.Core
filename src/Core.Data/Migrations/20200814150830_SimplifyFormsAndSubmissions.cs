using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class SimplifyFormsAndSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Forms_FormId",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "Forms",
                schema: "CMS");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_FormId",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_render");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "form_update");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "metaitem_update");

            migrationBuilder.DropColumn(
                name: "FormId",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "CMS",
                table: "Submissions",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "CMS",
                table: "Submissions",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Submissions",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedOn",
                schema: "CMS",
                table: "Submissions",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "SourceComponent",
                schema: "CMS",
                table: "Submissions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "CMS",
                table: "Submissions",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "submission_create", "Allows users to Create Submissions.", "Create", false, "Submission" },
                    { "submission_read", "Allows users to Read Submissions.", "Read", false, "Submission" },
                    { "submission_update", "Allows users to Update Submissions.", "Update", false, "Submission" },
                    { "submission_delete", "Allows users to Delete Submissions.", "Delete", false, "Submission" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "submission_create");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "submission_delete");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "submission_read");

            migrationBuilder.DeleteData(
                schema: "Security",
                table: "Privileges",
                keyColumn: "Id",
                keyValue: "submission_update");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SourceComponent",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "CMS",
                table: "Submissions");

            migrationBuilder.AddColumn<Guid>(
                name: "FormId",
                schema: "CMS",
                table: "Submissions",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Forms",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    FieldTemplate = table.Column<string>(nullable: true),
                    FieldsetTemplate = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RawMetaJson = table.Column<string>(nullable: true),
                    ResourceKey = table.Column<string>(nullable: true),
                    RootMetaItem = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "form_create", "Allows users to Create Forms.", "Create", false, "Form" },
                    { "form_read", "Allows users to Read Forms.", "Read", false, "Form" },
                    { "form_update", "Allows users to Update Forms.", "Update", false, "Form" },
                    { "form_delete", "Allows users to Delete Forms.", "Delete", false, "Form" },
                    { "form_render", "Allows users to call Render on Forms.", "Render", false, "Form" },
                    { "metaitem_create", "Allows users to Create MetaItems.", "Create", false, "MetaItem" },
                    { "metaitem_read", "Allows users to Read MetaItems.", "Read", false, "MetaItem" },
                    { "metaitem_update", "Allows users to Update MetaItems.", "Update", false, "MetaItem" },
                    { "metaitem_delete", "Allows users to Delete MetaItems.", "Delete", false, "MetaItem" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FormId",
                schema: "CMS",
                table: "Submissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_AppId",
                schema: "CMS",
                table: "Forms",
                column: "AppId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Forms_FormId",
                schema: "CMS",
                table: "Submissions",
                column: "FormId",
                principalSchema: "CMS",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
