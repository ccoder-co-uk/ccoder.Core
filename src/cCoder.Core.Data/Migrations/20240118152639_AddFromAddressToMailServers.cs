using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cCoder.Core.Migrations;

/// <inheritdoc />
public partial class AddFromAddressToMailServers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "FromEmail",
            schema: "Mail",
            table: "MailServers",
            type: "nvarchar(max)",
            nullable: true);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "FromEmail",
            schema: "Mail",
            table: "MailServers");
}
