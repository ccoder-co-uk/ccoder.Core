using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Packaging.Importers
{
    public class ComponentImporter : CoreImporter<Component>
    {
        public ComponentImporter(IComponentService service) : base(service, "Core/Component") { }

        public override async Task Import(int appId, PackageItem item)
        {
            Component[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Component>() } : item.Unpack<Component[]>();
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
}