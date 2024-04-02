using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Packaging;
using Core.Objects.Extensions;
using Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Packaging.Importers
{
    public class ResourceImporter : CoreImporter<Resource>
    {
        public ResourceImporter(IResourceService service) : base(service, "Core/Resource") { }

        public override async Task Import(int appId, PackageItem item)
        {
            Resource[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Resource>() } : item.Unpack<Resource[]>();

            var dbVersions = Service.GetAll(false)
                .Where(c => c.AppId == appId).Select(r => new { r.Id, r.Name, Match = $"{r.Key}_{r.Name}_{r.Culture}" })
                .ToArray();

            items.ForEach(l =>
            {
                l.AppId = appId;
                l.Id = dbVersions.FirstOrDefault(i => $"{l.Key}_{l.Name}_{l.Culture}" == i.Match)?.Id ?? 0;
            });

            _ = await Service.AddOrUpdate(items);
        }
    }
}