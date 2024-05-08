using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Packaging.Importers;

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
        PageRoleInfo[] pageRoles = item.Data != null && item.Data.StartsWith("{")
            ? new[] { item.Unpack<PageRoleInfo>() }
            : item.Unpack<PageRoleInfo[]>();

        Role[] roles = Db.GetAll<Role>(false)
            .Where(r => r.AppId == appId)
            .ToArray();

        Page[] pages = Db.GetAll<Page>(false)
            .Where(r => r.AppId == appId)
            .Include(k => k.Roles)
                .ThenInclude(r => r.Role)
            .ToArray();

        PageRole[] newPageRoles = pageRoles.Select(fr =>
        {
            Page page = pages.FirstOrDefault(f => f.Path == fr.Path);
            Role role = roles.FirstOrDefault(r => r.Name == fr.Role);

            return new PageRole { PageId = page?.Id ?? default, RoleId = role?.Id ?? Guid.Empty };

        }).ToArray();

        _ = await Service.AddAllAsync(newPageRoles.Where(pr => pr.PageId != default && pr.RoleId != Guid.Empty));
    }
}