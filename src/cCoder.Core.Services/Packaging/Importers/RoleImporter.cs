using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services.Packaging.Importers;

public class RoleImporter : CoreImporter<Role>
{
    protected ICoreDataContext Db { get; }

    public RoleImporter(ICoreService<Role> service, ICoreDataContext db) : base(service, "Core/Role")
    {
        Db = db;
    }

    public override async Task Import(int appId, PackageItem item)
    {
        Role[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Role>() } : item.Unpack<Role[]>();

        items.ForEach(l => l.AppId = appId);

        var dbVersions = Db.GetAll<Role>(false).Where(c => c.AppId == appId).Select(r => new { r.Id, r.Name }).ToArray();

        items.ForEach(l =>
        {
            l.AppId = appId;
            l.Id = dbVersions.FirstOrDefault(i => i.Name == l.Name)?.Id ?? Guid.Empty;
        });

        _ = await Service.AddOrUpdate(items, false);
    }
}