using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddDefaultNameValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Per SET Per.[Name]=(SELECT Title FROM [CMS].[PageInfo] pii WHERE pii.CultureId='' AND pii.PageId=Per.Id) FROM [Core].[CMS].[Pages] Per WHERE LEN(Per.[Name]) = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty
        }
    }
}
