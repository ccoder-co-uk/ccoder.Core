using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Packaging;
using Core.Objects.Extensions;
using Core.Services;
using Path = Core.Objects.Path;

namespace Core.Packaging.Importers
{
    public class PageImporter : CoreImporter<Page>
    {
        protected ICoreDataContext Db { get; }

        public PageImporter(IPageService service, ICoreDataContext db) : base(service, "Core/Page") => Db = db;

        public override async Task Import(int appId, PackageItem item)
        {
            Page[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Page>() } : item.Unpack<Page[]>();
            items = items.OrderBy(x => x.Path.Split('/').Length).ToArray();
            string[] names = items.Select(l => l.Name.ToLower()).ToArray();
            var dbVersions = Service.GetAll(false)
                .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
                .Select(l => new { l.Id, l.Path })
                .ToArray();

            items.ForEach(i =>
            {
                i.AppId = appId;
                string parentPath = (new Path(i.Path)).ParentPath.FullPath;
                i.ParentId = Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == parentPath.ToLower() && p.AppId == appId && !string.IsNullOrEmpty(p.Path))?.Id;
                i.Id = dbVersions.FirstOrDefault(j => j.Path.ToLower() == i.Path.ToLower())?.Id ?? 0;
            });

            _ = await Service.AddOrUpdate(items);
        }
    }
}