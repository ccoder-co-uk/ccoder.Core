using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Path = cCoder.Core.Objects.Path;

namespace cCoder.Core.Services.CMS;

public class PageService : CoreService<Page>, IPageService
{
    private readonly Config config;

    public PageService(ICoreDataContext db, Config config) : base(db)
    {
        this.config = config;
    }

    public RenderResult Render(int appId, string path, string theme, string culture, bool edit = false)
    {
        path ??= string.Empty;
        culture ??= User.DefaultCultureId;

        App app = Db.GetAll<App>(false)
            .Include(p => p.Pages)
                .ThenInclude(p => p.PageInfo)
            .Include(a => a.Roles)
            .Include(a => a.Layouts)
            .Include(a => a.Templates)
            .Include(a => a.Components)
            .Include(a => a.Resources)
            .Include(a => a.Scripts)
            .AsSplitQuery()
            .FirstOrDefault(a => a.Id == appId);

        if (app == null)
            throw new SecurityException("Unknown Domain!");

        Db.DisableFilters();
        Page page = Db.GetAll<Page>(false)
            .Include(p => p.PageInfo)
            .Include(p => p.Contents)
            .Include(p => p.Roles)
                .ThenInclude(pr => pr.Role)
            .AsSplitQuery()
            .FirstOrDefault(p => p.AppId == appId && p.Path.ToLower() == path.ToLower());

        Db.EnableFilters();

        if (page != null)
            page.App = app;

        if (page == null)
        {
            // page just doesn't exist ...
            RenderResult notFoundRenderResult = new Page
            {
                App = app,
                Path = path,
                PageInfo = new[] { new PageInfo { Title = "Page Not Found", Description = "Page Not Found", Keywords = "Page Not Found", CultureId = string.Empty } },
                Contents = new[] { new Content { CultureId = culture, Html = "[component[NotFound]]" } }
            }
            .ToRenderResult(config, Db, User, theme, culture);

            notFoundRenderResult.StatusCode = 404;
            return notFoundRenderResult;
        }

        else if (!page.UserCan(User, "page_read") && !User.IsAdminOfApp(appId))
        {
            // denied ...
            IEnumerable<string> names = page.Contents.Select(c => c.Name).Distinct();

            page.Contents = new Content[]
            {
                new() { CultureId = "", Html = "[component[login]]", Name = names.First() }
            };

            return page.ToRenderResult(config, Db, User, theme, culture, false);
        }
        else // regular render ...
            return page.ToRenderResult(config, Db, User, theme, culture, edit && page.UserCan(User, "page_update"));
    }

    public string MenuFor(int id, string culture)
    {
        IEnumerable<string> pages = GetAll()
            .AsQueryable()
            .Include(p => p.PageInfo)
            .Where(p => p.ParentId == id && p.ShowOnMenus)
            .OrderBy(p => p.Order)
            .AsEnumerable()
            .Select(s => $"<li data-id='{s.Id}' class='item'><a href='/{s.Path}'>{s.Title(culture)}</a></li>");

        string subs = pages.Any() ? string.Join("", pages) : string.Empty;
        return $"<ul class='submenu'>{subs}</ul>";
    }

    public Page GetRoot(int id)
    {
        Page page = Get(id);
        while (page.ParentId != null)
        {
            Page parent = Get(page.ParentId.Value);
            page = parent ?? page;
        }
        return page;
    }

    public IEnumerable<Page> GetChildren(int id) => GetAll().Where(p => p.ParentId == id);

    public override Task DeleteAsync(object id) => UserCan("page_delete", (int)id)
        ? Db.DeletePage((int)id)
        : throw new SecurityException("Access Denied!");

    public override async Task<Page> UpdateAsync(Page page)
    {
        Page dbVersion = GetDbVersionForUpdate(page);

        if (dbVersion == null || !UserCan("page_update", dbVersion.Id))
            throw new SecurityException("Access Denied!");

        await UpdatePageParts(page, dbVersion);

        // If Page has a new parent, then we need to get the new Parent in order to use it's path in RecomputePaths()
        if (dbVersion.ParentId != page.ParentId && page.ParentId != null)
        {
            // Add Parent to dbVersion in order for RecomputePaths() to use it's path
            Page newParent = Db.GetAll<Page>(true)
                .FirstOrDefault(p => p.Id == page.ParentId);

            dbVersion.Parent = newParent ?? throw new SecurityException("Access Denied");
        }

        dbVersion.UpdateFrom(page);
        dbVersion.RecomputePaths();

        dbVersion.LastUpdated = DateTimeOffset.Now;
        dbVersion.LastUpdatedBy = Db.User.Id;

        await Db.SaveChangesAsync();

        return dbVersion;
    }

    private Page GetDbVersionForUpdate(Page page)
    {
        IQueryable<Page> query = GetAll(true)
            .AsQueryable()
            .Include(p => p.PageInfo)
            .Include(p => p.Parent)
            .AsQueryable();

        if (page.Contents != null && page.Contents.Any())
            query = query.Include(p => p.Contents);

        if (page.Roles != null && page.Roles.Any())
            query = query.Include(p => p.Roles);

        Page dbVersion = query.FirstOrDefault(p => p.Id == page.Id);

        return dbVersion;
    }

    private async Task UpdatePageParts(Page page, Page dbVersion)
    {
        if (page.PageInfo != null && page.PageInfo.Any())
            await dbVersion.UpdateInfo(User, page.PageInfo, Db);

        if (page.Contents != null && page.Contents.Any())
            await dbVersion.UpdateContents(User, page.Contents, Db);

        if (page.Roles != null && page.Roles.Any())
            await dbVersion.UpdateRoles(User, page.Roles, Db);
    }

    public override async Task<Page> AddAsync(Page page)
    {
        if (page.Pages != null)
            throw new ValidationException("Can only import one page at a time.");
        if (page.PageInfo?.Any(pi => pi.CultureId == string.Empty) != true)
            throw new ValidationException("Pages MUST have page information defined for the default culture, other cultures are optional.");

        // compute some variables 
        bool userIsAppAdmin = User.IsAdminOfApp(page.AppId);
        bool userCanCreateChildOfParent = userIsAppAdmin || (page.ParentId != null && UserCan("page_create", (int)page.ParentId));

        // grab the parent page
        Page parent = null;
        if (page.ParentId != null)
        {
            Db.DisableFilters();
            parent = Db.GetAll<Page>(false)
                .Include(p => p.Roles)
                .FirstOrDefault(p => p.Id == page.ParentId);
            Db.EnableFilters();
        }
        else if (page.Path != null && page.Path.Contains('/'))
        {
            string parentPath = new Path(page.Path).ParentPath.FullPath;
            parent = Db.GetAll<Page>(false)
                .Include(p => p.Roles)
                .FirstOrDefault(p => p.Path.ToLower() == parentPath.ToLower() && p.AppId == page.AppId);
        }

        if (parent != null)
            page.Path = parent.Path + "/" + page.InfoForCulture(string.Empty).Title.Replace(" ", string.Empty);
        else
            page.Path = page.Name != "Home"
                ? page.InfoForCulture(string.Empty).Title.Replace(" ", string.Empty)
                : string.Empty;

        page.ParentId = parent?.Id;
        Page newPage = new Page().UpdateFrom(page);

        // create the page
        Page result = await base.AddAsync(newPage);
        result.Roles = parent?.Roles
            .Select(r => new PageRole { RoleId = r.RoleId })
            .ToArray();

        SetRolesOnNewPage(page, parent, result);

        // add meta

        await SetPageInfoOnNewPage(page, result);
        await SetContentsOnNewPage(page, result);

        await Db.SaveChangesAsync();

        return result;
    }

    private async Task SetPageInfoOnNewPage(Page page, Page result)
    {
        IEnumerable<PageInfo> pageInfoCopy = page.PageInfo.Select(p => new PageInfo()
        {
            CultureId = p.CultureId,
            Description = p.Description,
            Keywords = p.Keywords,
            PageId = result.Id,
            Title = p.Title
        }).ToList();

        result.PageInfo = (await Db.AddAllAsync(pageInfoCopy)).ToList();
    }

    private async Task SetContentsOnNewPage(Page page, Page result)
    {
        if (page.Contents != null && page.Contents.Any())
        {
            // add contents
            page.Contents.ForEach(x =>
            {
                x.Id = 0;
                x.PageId = result.Id;
            });

            result.Contents = (await Db.AddAllAsync(page.Contents)).ToList();
        }
    }

    private void SetRolesOnNewPage(Page page, Page parent, Page result)
    {
        if (page.Roles == null || !page.Roles.Any())
        {
            result.Roles = parent != null
                ? (parent?.Roles
                    .Select(r => new PageRole { RoleId = r.RoleId })
                    .ToArray())
                : (ICollection<PageRole>)User.Roles
                    .Where(r => r.Role.AppId == page.AppId)
                    .Select(ur => new PageRole { RoleId = ur.RoleId })
                    .ToArray();

            result.Roles.ForEach(x => { x.PageId = result.Id; });
        }
    }

    private async Task CreateChildren(Page page, Page result)
    {
        if (page.Pages != null && page.Pages.Any())
            foreach (Page p in page.Pages)
            {
                p.ParentId = result.Id;
                _ = p.Id != 0 ? await UpdateAsync(p) : await AddAsync(p);
            }
    }

    private bool UserCan(string privKey, int pageId)
        => GetAll()
            .Include(p => p.Roles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefault(p => p.Id == pageId)?
            .UserCan(User, privKey) ?? false;

    public async Task RecomputeAllForAppAsync(int appId)
    {
        if (!User.IsAdminOfApp(appId))
            throw new SecurityException("Access Denied!");

        Page[] pages = GetAll(true)
            .Include(p => p.Parent)
            .Include(p => p.PageInfo)
            .Where(p => p.AppId == appId)
            .ToArray();

        RecomputePathForPages(null, pages);
        await Db.SaveChangesAsync();
    }

    private void RecomputePathForPages(int? parentId, Page[] pages)
        => pages.Where(p => p.ParentId == parentId)
        .ForEach(p =>
            {
                p.RecomputePaths();
                RecomputePathForPages(p.Id, pages);
            });
}
