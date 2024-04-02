using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Packaging;
using Core.Objects.Entities.Security;
using Core.Objects.Extensions;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Packaging.Importers
{
    public class PageRoleImporter : CoreImporter<PageRole>
    {
        protected ICoreDataContext Db { get; }

        public PageRoleImporter(ICoreService<PageRole> service, ICoreDataContext db) : base(service, "Core/PageRole") { Db = db; Order = 2; }

        public class PageRoleInfo
        {
            public string Path { get; set; }
            public string Role { get; set; }
        }

        public override async Task Import(int appId, PackageItem item)
        {
            var pageRoles = item.Data != null && item.Data.StartsWith("{")
                ? new[] { item.Unpack<PageRoleInfo>() }
                : item.Unpack<PageRoleInfo[]>();

            var roles = Db.GetAll<Role>(false)
                .Where(r => r.AppId == appId)
                .ToArray();

            var pages = Db.GetAll<Page>(false)
                .Where(r => r.AppId == appId)
                .Include(k => k.Roles)
                    .ThenInclude(r => r.Role)
                .ToArray();

            var newPageRoles = pageRoles.Select(fr =>
            {
                Page page = pages.FirstOrDefault(f => f.Path == fr.Path);
                Role role = roles.FirstOrDefault(r => r.Name == fr.Role);

                return new PageRole { PageId = page?.Id ?? default, RoleId = role?.Id ?? System.Guid.Empty };

            }).ToArray();

            _ = await Service.AddAllAsync(newPageRoles.Where(pr => pr.PageId != default && pr.RoleId != System.Guid.Empty));
        }
    }
}