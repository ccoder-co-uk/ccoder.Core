using Core.Objects.Entities.Packaging;
using Core.Objects.Entities.Workflow;
using Core.Objects.Extensions;
using Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Packaging.Importers
{
    public class BusinessProcessImporter : CoreImporter<BusinessProcess>
    {
        public BusinessProcessImporter(ICoreService<BusinessProcess> service) : base(service, "Core/BusinessProcess") { }

        public override async Task Import(int appId, PackageItem item)
        {
            BusinessProcess[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<BusinessProcess>() } : item.Unpack<BusinessProcess[]>();
            string[] names = items.Select(l => l.Name.ToLower()).ToArray();
            var dbVersions = Service.GetAll(false)
                .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
                .Select(l => new { l.Id, l.Name })
                .ToArray();

            items.ForEach(i =>
            {
                var db = dbVersions.FirstOrDefault(j => j.Name.ToLower() == i.Name.ToLower());
                i.AppId = appId;
                i.Id = db != null ? db.Id : i.Id;
            });


            _ = await Service.AddOrUpdate(items);
        }
    }
}