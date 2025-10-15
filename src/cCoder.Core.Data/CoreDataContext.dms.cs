using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Data;

public partial class CoreDataContext
{
    // Document Management
    public virtual DbSet<Folder> Folders { get; set; }
    public virtual DbSet<File> Files { get; set; }
    public virtual DbSet<FileContent> FileContents { get; set; }

    // Join entities
    public virtual DbSet<FolderRole> FolderRoles { get; set; }

    public async Task DeleteFolder(Guid folderId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        string script = @"
DECLARE @tree TABLE
(
	Id uniqueidentifier, 
	[Path] nvarchar(max), 
	Depth int
);
DECLARE @allFiles TABLE (Id uniqueidentifier);
 
WITH folderTree (Id, [Path], Depth) 
AS (
	SELECT Id, [Path], len([Path]) - len(replace([Path],'/','')) as Depth 
		FROM DMS.Folders
		WHERE Id = @p0
	UNION ALL
		SELECT f.Id, f.[Path], len(f.[Path]) - len(replace(f.[Path],'/',''))  
			FROM DMS.Folders f
            INNER JOIN folderTree cte ON cte.Id = f.ParentId
)

INSERT INTO @tree
SELECT * 
	FROM folderTree
	ORDER by Depth desc;

INSERT INTO @allFiles
    SELECT Id FROM [DMS].Files
        WHERE FolderId IN (
            SELECT Id FROM @tree
        );

UPDATE [DMS].Files
    SET DeletedOn = @p1
        WHERE Id IN (SELECT Id FROM @allFiles);

WHILE EXISTS(SELECT * FROM @tree)
BEGIN
    DELETE FROM [Security].[FolderRoles] WHERE FolderId IN (SELECT Id FROM @tree);
    UPDATE [DMS].Folders
        SET DeletedOn = @p1
            WHERE Id IN (SELECT Id FROM @tree WHERE Depth = (SELECT Max(Depth) FROM @tree));
	DELETE FROM @tree WHERE Depth = (SELECT Max(Depth) FROM @tree);
END
            ";

        log.LogDebug($"Dropping folder {folderId}");
        _ = await Database.ExecuteSqlRawAsync(script, new object[] { folderId, now });
        log.LogDebug($"Folder {folderId} Drop complete!");
    }

    public void DeleteFile(Guid fileId)
    {
        log.LogDebug($"Dropping file {fileId}");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        _ = Database.ExecuteSqlRaw($"UPDATE [DMS].Files SET DeletedOn = @p1 WHERE Id = @p0;", new object[] { fileId, now });
        log.LogDebug($"File {fileId} drop complete");
    }
}