using Core.Objects.Entities.Packaging;
using Core.Objects.Entities.Planning;
using Core.Objects.Extensions;
using Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Packaging.Importers
{
    public class CalendarImporter : CoreImporter<Calendar>
    {
        public CalendarImporter(ICoreService<Calendar> service) : base(service, "Core/Calendar") { }

        public override async Task Import(int appId, PackageItem item)
        {
            Calendar[] items = item.Data.StartsWith("{") ? new[] { item.Unpack<Calendar>() } : item.Unpack<Calendar[]>();
            string[] names = items.Select(l => l.Name.ToLower()).ToArray();
            var dbVersions = Service.GetAll(false)
                .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
                .Select(l => new { l.Id, l.Name })
                .ToArray();

            items.ForEach(l =>
            {
                l.AppId = appId;
                l.Id = dbVersions.FirstOrDefault(i => i.Name.ToLower() == l.Name.ToLower())?.Id ?? 0;
            });

            _ = await Service.AddAllAsync(items.Where(i => i.Id == 0));
        }
    }
}