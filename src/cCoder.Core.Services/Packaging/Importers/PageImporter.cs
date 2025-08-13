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

        foreach (Page page in items)
        {
            page.AppId = appId;
            Page parent;

            if(page.Path.StartsWith('/'))
            {
                parent = page.Path.Split('/').Count() == 2
                    ? Db.GetAll<Page>().FirstOrDefault(p => p.Path == "" && p.AppId == appId) // Import a page that is a child of /.
                    : Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == page.Path.ToLower().TrimStart('/') && p.AppId == appId); // Import a page that is a child of a child of /.

                page.ParentId = parent?.Id;
                page.Path = page.Path.TrimStart('/');
            } else
            {
                string parentPath = new Path(page.Path).ParentPath.FullPath;
                parent = Db.GetAll<Page>().FirstOrDefault(p => p.Path.ToLower() == parentPath.ToLower() && p.AppId == appId);

                page.ParentId = parent?.Id;
            }

            page.Id = dbVersions.FirstOrDefault(j => j.Path.ToLower() == page.Path.ToLower())?.Id ?? 0;

            await Service.AddOrUpdate([page]);
        }
    }
}