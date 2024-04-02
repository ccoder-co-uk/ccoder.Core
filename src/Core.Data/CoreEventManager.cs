using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.DMS;
using Core.Objects.Entities.Security;
using Microsoft.Extensions.Logging;
using File = Core.Objects.Entities.DMS.File;
using Path = Core.Objects.Path;

namespace Core
{
    public class CoreEventManager : EventManager
    {

        public CoreEventManager(ILogger log, ICoreDataContext core, Config config, ICoreAuthInfo auth) 
            : base(log, core, config, auth, "Core") { }

        public override async Task RaiseEvent<T>(T forObject, string name)
        {
            var ignoreTypes = new[] { typeof(PageInfo), typeof(PageRole), typeof(FolderRole) };

            if (ignoreTypes.Contains(forObject.GetType()))
                return;

            // Compute the context in to which the event occurred
            int? appId = (int?)Core.GetType(
                ).GetMethod("GetAppId")
                .MakeGenericMethod(forObject.GetType())
                .Invoke(Core, new[] { forObject });

            string appContext = $"App({appId})";

            if (appId != null)
            {
                await (forObject switch
                {
                    FileContent fc => fc.File != null
                        ? base.RaiseEvent(fc.File, $"{appContext}/DMS(File_Updated)/{new Path(fc.File.Path).ParentPath.FullPath}")
                        : base.RaiseEvent(Core.Get<File>(fc.FileId), $"App({appId})/DMS(File_Updated)/{new Path(fc.File?.Path).ParentPath.FullPath}"),
                    File f => base.RaiseEvent(f, $"{appContext}/DMS(File_{name})/{new Path(f.Path).ParentPath.FullPath}"),
                    Content pc => pc.Page != null
                        ? base.RaiseEvent(pc.Page, $"{appContext}/Page_Updated")
                        : base.RaiseEvent(Core.Get<Page>(pc.PageId), $"App({appContext})/Page_Updated"),
                    PageInfo pi => pi.Page != null
                        ? base.RaiseEvent(pi.Page, $"{appContext}/Page_Updated")
                        : base.RaiseEvent(Core.Get<Page>(pi.PageId), $"App({appContext})/Page_Updated"),
                    _ => base.RaiseEvent(forObject, $"{appContext}/{forObject.GetType().Name}_{name}")
                });
            }
        }
    }
}