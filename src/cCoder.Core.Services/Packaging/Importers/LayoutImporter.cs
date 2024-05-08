using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services.Packaging.Importers;

public class LayoutImporter : CoreImporter<Layout>
{
    public LayoutImporter(ICoreService<Layout> service) : base(service, "Core/Layout") { }

    public override async Task Import(int appId, PackageItem item)
    {
        Layout[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Layout>() } : item.Unpack<Layout[]>();
        string[] names = items.Select(l => l.Name.ToLower()).ToArray();
        var dbVersions = Service.GetAll(false)
            .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
            .Select(l => new { l.Id, l.Name })
            .ToArray();

        items.ForEach(i =>
        {
            i.AppId = appId;
            i.Id = dbVersions.FirstOrDefault(j => j.Name.ToLower() == i.Name.ToLower())?.Id ?? 0;
        });

        _ = await Service.AddOrUpdate(items);
    }
}