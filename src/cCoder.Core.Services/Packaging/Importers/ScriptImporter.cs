using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services.Packaging.Importers;

public class ScriptImporter : CoreImporter<Script>
{
    public ScriptImporter(IScriptService service) : base(service, "Core/Script") { }

    public override async Task Import(int appId, PackageItem item)
    {
        Script[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Script>() } : item.Unpack<Script[]>();

        string[] names = items.Select(l => l.Name.ToLower()).ToArray();
        var dbVersions = Service.GetAll(false)
            .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
            .Select(l => new { l.Id, l.Name })
            .ToArray();

        items.ForEach(l =>
        {
            l.AppId = appId;
            l.Id = dbVersions.FirstOrDefault(i => i.Name.ToLower() == l.Name.ToLower())?.Id ?? default;
        });

        _ = await Service.AddOrUpdate(items);
    }
}