using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Extensions;
using Path = cCoder.Core.Objects.Path;

namespace cCoder.Core.Services.Packaging.Importers;

public class PageImporter : CoreImporter<Page>
{
    protected ICoreDataContext Db { get; }

    public PageImporter(IPageService service, ICoreDataContext db) : base(service, "Core/Page")
    {
        Db = db;
    }

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
            string parentPath = new Path(i.Path).ParentPath.FullPath;
            i.ParentId = i.Path.Contains('/')
                ? Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == parentPath.ToLower() && p.AppId == appId)?.Id
                : null;
            i.Id = dbVersions.FirstOrDefault(j => j.Path.ToLower() == i.Path.ToLower())?.Id ?? 0;
        });

        _ = await Service.AddOrUpdate(items);
    }
}