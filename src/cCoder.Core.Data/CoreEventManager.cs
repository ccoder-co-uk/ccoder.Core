using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using File = cCoder.Core.Objects.Entities.DMS.File;
using Path = cCoder.Core.Objects.Path;

namespace cCoder.Core.Data;

public class CoreEventManager : EventManager, ICoreEventManager
{
    private bool loadedEvents = false;

    public CoreEventManager(ILogger log, ICoreDataContext core, Config config, ICoreAuthInfo auth)
        : base(log, core, config, auth, "Core") { }

    public override async Task RaiseEvent<T>(T forObject, string name)
    {
        if (!loadedEvents)
        {
            Core.DisableFilters();

            Subscriptions = Core.GetAll<WorkflowEvent>(false)
                .Include(sub => sub.Flow)
                .Include(sub => sub.ExecuteAsUser)
                    .ThenInclude(u => u.Roles)
                        .ThenInclude(r => r.Role)
                .Where(e => e.Type.StartsWith("Core"))
                .ToArray();

            Core.EnableFilters();

            loadedEvents = true;
        }

        Type[] ignoreTypes = new[] { typeof(PageInfo), typeof(PageRole), typeof(FolderRole) };

        if (ignoreTypes.Contains(forObject.GetType()))
            return;

        // Compute the context in to which the event occurred
        int? appId = (int?)Core.GetType()
            .GetMethod("GetAppId")
            .MakeGenericMethod(forObject.GetType())
            .Invoke(Core, new[] { forObject });

        string appContext = $"App({appId})";

        if (appId != null)
            await (forObject switch
            {
                FileContent fc => fc.File != null
                    ? base.RaiseEvent(CreateEventSafeFile(fc.File), $"{appContext}/DMS(File_Updated)/{new Path(fc.File.Path).ParentPath.FullPath}")
                    : base.RaiseEvent(CreateEventSafeFile(Core.Get<File>(fc.FileId)), $"App({appId})/DMS(File_Updated)/{new Path(fc.File?.Path).ParentPath.FullPath}"),
                File f => base.RaiseEvent(CreateEventSafeFile(f), $"{appContext}/DMS(File_{name})/{new Path(f.Path).ParentPath.FullPath}"),
                Content pc => pc.Page != null
                    ? base.RaiseEvent(pc.Page, $"{appContext}/Page_Updated")
                    : base.RaiseEvent(Core.Get<Page>(pc.PageId), $"App({appContext})/Page_Updated"),
                PageInfo pi => pi.Page != null
                    ? base.RaiseEvent(pi.Page, $"{appContext}/Page_Updated")
                    : base.RaiseEvent(Core.Get<Page>(pi.PageId), $"App({appContext})/Page_Updated"),
                _ => base.RaiseEvent(forObject, $"{appContext}/{forObject.GetType().Name}_{name}")
            });
    }

    private static File CreateEventSafeFile(File file)
    {
        if (file == null)
            return null;

        return new File
        {
            Id = file.Id,
            FolderId = file.FolderId,
            Name = file.Name,
            Description = file.Description,
            Path = file.Path,
            MimeType = file.MimeType,
            CreatedBy = file.CreatedBy,
            Size = file.Size,
            CreatedOn = file.CreatedOn,
            DeletedOn = file.DeletedOn,
            Folder = CreateEventSafeFolder(file.Folder),
            Contents = file.Contents.Select(CreateEventSafeFileContent).ToArray()
        };
    }

    private static FileContent CreateEventSafeFileContent(FileContent content) =>
        new FileContent
        {
            Id = content.Id,
            FileId = content.FileId,
            Description = content.Description,
            Size = content.Size,
            CreatedBy = content.CreatedBy,
            CreatedOn = content.CreatedOn,
            Version = content.Version,
            RawData = content.RawData,
            File = null
        };

    private static Folder CreateEventSafeFolder(Folder folder)
    {
        if (folder == null)
            return null;

        return new Folder
        {
            Id = folder.Id,
            AppId = folder.AppId,
            ParentId = folder.ParentId,
            Name = folder.Name,
            Path = folder.Path,
            DeletedOn = folder.DeletedOn
        };
    }
}
