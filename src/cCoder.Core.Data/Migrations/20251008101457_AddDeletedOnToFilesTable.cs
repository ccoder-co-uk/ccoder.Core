using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedOnToFilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOn",
                schema: "DMS",
                table: "Files",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "DMS",
                table: "Files");
        }
    }
}
