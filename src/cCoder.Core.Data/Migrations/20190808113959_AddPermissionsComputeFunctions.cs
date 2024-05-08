using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class AddPermissionsComputeFunctions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE FUNCTION [DMS].[GetFolderPrivList] 
(	
	@UserId nvarchar(450),
	@FolderId uniqueidentifier
)
RETURNS @PrivList TABLE ([Value] nvarchar(100) NOT NULL)
AS
BEGIN
	INSERT INTO @PrivList ([Value])
		SELECT DISTINCT *
		  FROM STRING_SPLIT
		  ( 
			  (
				SELECT STUFF(
					(
						SELECT R.Privs 
							FROM DMS.Folders F
							JOIN [Security].[FolderRoles] FR ON FR.FolderId = F.Id
							JOIN [Security].[Roles] R ON R.Id = FR.RoleId
							JOIN [Security].[UserRoles] UR ON UR.RoleId = R.Id
							WHERE F.Id = @FolderId AND UR.UserId = @UserId
						FOR XML PATH('')
						), 1, 1, ''
					)
				),','
			) WHERE [value] LIKE 'folder_%' OR [value] like 'file_%';

	IF (SELECT ParentId FROM [DMS].[Folders] WHERE Id = @FolderId) IS NOT NULL
	BEGIN
		INSERT INTO @PrivList ([Value])
			SELECT [Value]
			  FROM [DMS].[GetFolderPrivList](@UserId,(SELECT ParentId FROM [DMS].[Folders] WHERE Id = @FolderId))
			  WHERE [Value] NOT IN (SELECT [Value] FROM @PrivList)
	END
	RETURN
END
            ");

        migrationBuilder.Sql(@"
CREATE FUNCTION [CMS].[GetPagePrivList] 
(	@UserId nvarchar(450),
	@PageId int
)
RETURNS @PrivList TABLE ([Value] nvarchar(100) NOT NULL)
AS
BEGIN
	INSERT INTO @PrivList ([Value])
		SELECT DISTINCT *
          FROM STRING_SPLIT
		  ( 
			  (
				SELECT STUFF(
					(
						SELECT ',' + R.Privs 
							FROM CMS.Pages P
							JOIN [Security].[PageRoles] PR ON PR.PageId = P.Id
							JOIN [Security].[Roles] R ON R.Id = PR.RoleId
							JOIN [Security].[UserRoles] UR ON UR.RoleId = R.Id
							WHERE P.Id = @PageId AND UR.UserId = @UserId
							FOR XML PATH('')
					), 1, 1, ''
				)
			),','
		) WHERE [value] LIKE 'page_%';

	IF (SELECT ParentId FROM CMS.Pages WHERE Id = @PageId) IS NOT NULL
	BEGIN
		INSERT INTO @PrivList ([Value])
			SELECT [Value]
			  FROM [CMS].[GetPagePrivList](@UserId,(SELECT Top 1 ParentId FROM CMS.Pages WHERE Id = @PageId))
			  WHERE [Value] NOT IN (SELECT [Value] FROM @PrivList)
	END	--*/
	RETURN
END
            ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"IF OBJECT_ID (N'CMS.GetPagePrivList', N'TF') IS NOT NULL DROP FUNCTION [CMS].[GetPagePrivList]");
        migrationBuilder.Sql(@"IF OBJECT_ID (N'DMS.GetFolderPrivList', N'TF') IS NOT NULL DROP FUNCTION [DMS].[GetFolderPrivList]");
    }
}
