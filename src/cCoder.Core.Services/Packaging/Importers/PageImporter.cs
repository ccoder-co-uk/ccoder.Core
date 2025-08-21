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
            .Where(c => c.AppId == appId)
            .ToArray();

        foreach (Page page in items)
        {
            page.AppId = appId;

            string parentPath = page.Path.Contains('/')
                ? new Path(page.Path).ParentPath.FullPath
                : null;

            Page parent = parentPath is not null
                ? Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == parentPath.ToLower() && p.AppId == appId)
                : null;

            page.ParentId = parent?.Id;

            page.Id = Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == page.Path.TrimStart('/').ToLower() && p.AppId == appId)?.Id ?? 0;

            await Service.AddOrUpdate([page]);
        }
    }
}