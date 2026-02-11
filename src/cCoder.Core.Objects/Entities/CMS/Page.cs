using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("Pages", Schema = "CMS")]
public class Page : IAmRoleSecured<PageRole>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Parent")]
    public int? ParentId { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    [DefaultValue(0)]
    public int Order { get; set; }

    [DefaultValue(true)]
    public bool ShowOnMenus { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [DefaultValue(typeof(DateTime), "2021-02-19")]
    public DateTimeOffset LastUpdated { get; set; }

    [StringLength(100)]
    public string LastUpdatedBy { get; set; }

    [DefaultValue(typeof(DateTime), "2021-02-19")]
    public DateTimeOffset CreatedOn { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    public string Path { get; set; }

    public string ResourceKey { get; set; }

    public string Layout { get; set; }

    public virtual App App { get; set; }

    public virtual Page Parent { get; set; }

    public virtual ICollection<PageInfo> PageInfo { get; set; }

    public virtual ICollection<Page> Pages { get; set; }

    public virtual ICollection<Content> Contents { get; set; }

    public virtual ICollection<PageRole> Roles { get; set; }



    [DontPrivilege]
    public void RecomputePaths()
    {
        string newPath = ParentId != null && !string.IsNullOrEmpty(Parent.Path)
            ? $"{Parent.Path}/{Title(string.Empty).Replace(" ", "")}"
            : $"{Title(string.Empty).Replace(" ", "")}";

        if (Path != string.Empty)
            Path = newPath;

        if (newPath != Path)
            Pages?.ForEach(p => p.RecomputePaths());
    }

    [DontPrivilege]
    public void SetContent(User user, Content content)
    {
        // eval can user do this
        if (App.IsAppAdmin(user) || UserCan(user, "page_update"))
        {
            // grab the "first or null" block of content thats already in the db
           Content existingVersion = Contents.FirstOrDefault(c => c.Name == content.Name);

            // if there is one, update it from the one passed in
            if (existingVersion != null)
            {
                content.Id = existingVersion.Id;
                content = existingVersion.UpdateFrom(content);
            }
            else
            {
                Contents.Add(content);
            }
        }
        else
        {
            throw new SecurityException("Access Denied!");
        }
    }

    [DontPrivilege]
    public string Title(string culture) 
        => InfoForCulture(culture).Title ?? string.Empty;

    [DontPrivilege]
    public string Description(string culture) 
        => InfoForCulture(culture).Description ?? string.Empty;

    [DontPrivilege]
    public string Keywords(string culture) 
        => InfoForCulture(culture).Keywords ?? string.Empty;

    [DontPrivilege]
    public PageInfo InfoForCulture(string culture)
    {
        culture ??= string.Empty;
        IOrderedEnumerable<PageInfo> orderedInfo = PageInfo.OrderByDescending(i => i.CultureId.Length);

        return orderedInfo.FirstOrDefault(i => culture == i.CultureId || culture.Contains(i.CultureId)) 
                ?? 
            orderedInfo.FirstOrDefault();
    }

    [DontPrivilege]
    public Content ContentForCulture(string name, string culture)
    {
        culture ??= string.Empty;

        Content result = Contents.Where(i => (i.CultureId?.Length ?? 0) <= culture.Length)
            .OrderByDescending(i => i.CultureId?.Length ?? 0)
            .FirstOrDefault(i => i.Name == name && culture.Contains(i.CultureId));

        result ??= Contents.FirstOrDefault(i => i.Name == name && string.IsNullOrEmpty(i.CultureId));
        return result ?? new Content { CultureId = string.Empty, Name = name, Html = string.Empty };
    }

    [DontPrivilege]
    public RenderResult ToRenderResult(Config config, ICoreDataContext ctx, User user, string theme, string culture, bool edit = false)
    {
        Layout layout = App.Layouts.FirstOrDefault(i => i.Name == Layout) ?? App.Layouts.First();
        PageInfo meta = InfoForCulture(culture);

        PageRenderParams p = new(ctx, this, (theme ?? App.DefaultTheme) ?? "Default", App, user, culture ?? string.Empty, edit);
        ICollection<Replacement> r = ContentHelper.DefaultReplacements(p, config);
        RenderResult result = new() { Theme = theme, Culture = culture };

        MergePage(result, this);
        MergeMeta(result, meta);


        if (((IDictionary<string, object>)App.Config).ContainsKey("Themes"))
        {
            Template baseTemplate = App?.Templates?.OrderByDescending(l => l.Name.Length).FirstOrDefault(l => l.Name == "Theme");
            Template themeTemplate = App?.Templates?.OrderByDescending(l => l.Name.Length).FirstOrDefault(l => l.Name == "Theme-" + p.Theme);
            string baseTheme = baseTemplate?.Render((IDictionary<string, object>)App.Config?.Themes, p, config);

            dynamic themes = ((IDictionary<string, object>)App.Config)["Themes"];
            var themeDictionary = themes as IDictionary<string, object>;
            string renderedTheme = themeTemplate?.Render(themeDictionary[p.Theme], p, config);

            r.Add(new Replacement("[theme[template]]", renderedTheme ?? ""));
            r.Add(new Replacement("[theme[base]]", baseTheme ?? ""));
        }

        result.Edit = edit;
        result.HeaderHtml = ContentHelper.ProcessContentString(string.IsNullOrEmpty(ResourceKey) ? "Default" : ResourceKey, p, layout.HeaderHtml, r);
        result.BodyHtml = ContentHelper.ProcessContentString(string.IsNullOrEmpty(ResourceKey) ? "Default" : ResourceKey, p, layout.Html, r);

        return result;
    }

    public async Task UpdateContents(User user, ICollection<Content> newContents, ICoreDataContext core)
    {
        if (!user.IsAdminOfApp(AppId) && !UserCan(user, "page_updatecontent"))
        {
            throw new SecurityException("Access denied!");
        }

        // apply updates
        Contents.ForEach(c => c.Html = newContents
            .FirstOrDefault(nc => c.Name == nc.Name && c.CultureId == nc.CultureId)?.Html ?? c.Html);

        // add the new stuff
        List<Content> addedContents = newContents
            .Where(nc => !Contents.Any(c => c.Name == nc.Name && c.CultureId == nc.CultureId)).ToList();

        addedContents.ForEach(c =>
        {
            c.PageId = Id;
            c.Id = 0;
            Contents.Add(c);
        });

        // remove the old stuff
        IEnumerable<Content> removedContents = Contents
            .Where(c => c.CultureId != string.Empty && !newContents.Any(nc => c.Name == nc.Name && c.CultureId == nc.CultureId));

        if (removedContents.Any())
        {
            Contents = Contents.Except(removedContents).ToList();
            await core.DeleteAllAsync(removedContents);
        }
    }

    public async Task UpdateRoles(User user, ICollection<PageRole> newRoles, ICoreDataContext core)
    {
        if (!user.IsAdminOfApp(AppId) && !UserCan(user, "page_updateroles"))
        {
            throw new SecurityException("Access denied!");
        }

        // add the new stuff
        IEnumerable<PageRole> addedRoles = newRoles?.Where(nr => !Roles.Any(r => r.PageId == nr.PageId && r.RoleId == nr.RoleId)) ?? Array.Empty<PageRole>();
        addedRoles.ForEach(r => Roles.Add(r));

        // remove the old stuff
        if (newRoles != null)
        {
            List<PageRole> removedPageRoles = Roles.Where(r => !newRoles.Any(nr => r.PageId == nr.PageId && r.RoleId == nr.RoleId)).ToList();
            if (removedPageRoles.Any())
            {
                Roles = Roles.Except(removedPageRoles).ToList();
                await core.DeleteAllAsync(removedPageRoles);
            }
        }
    }

    public async Task UpdateInfo(User user, ICollection<PageInfo> newInfo, ICoreDataContext core)
    {
        if (!user.IsAdminOfApp(AppId) && !UserCan(user, "page_updateinfo"))
        {
            throw new SecurityException("Access denied!");
        }

        // apply updates
        PageInfo.ForEach(c =>
        {
            PageInfo newVer = newInfo.FirstOrDefault(nc => c.CultureId == nc.CultureId);
            if (newVer != null)
            {
                c.Title = newVer.Title;
                c.Description = newVer.Description;
                c.Keywords = newVer.Keywords;
            }
        });

        // add the new stuff
        PageInfo[] addedInfo = newInfo?
            .Where(ni => ni.CultureId != string.Empty && !PageInfo.Any(pi => pi.CultureId == ni.CultureId))
            .ToArray() ?? [];

        addedInfo.ForEach(c =>
        {
            c.PageId = Id;
            c.Id = 0;
            if (!string.IsNullOrEmpty(c.CultureId))
            {
                PageInfo.Add(c);
            }
        });

        // remove the old stuff
        List<PageInfo> removedPageInfos = PageInfo
            .Where(c => c.CultureId != string.Empty && !newInfo.Any(nc => c.CultureId == nc.CultureId))
            .ToList();

        if (removedPageInfos.Any())
        {
            PageInfo = PageInfo.Except(removedPageInfos).ToList();
            await core.DeleteAllAsync(removedPageInfos);
        }
    }

    [DontPrivilege]
    public bool UserCan(User user, string priv)
    {
        Guid[] userRoles = user.Roles?.Select(r => r.RoleId).ToArray() ?? Array.Empty<Guid>();
        return user.IsAdminOfApp(AppId) || (Roles?.Where(pr => userRoles.Contains(pr.RoleId))
            .SelectMany(pr => pr.Role?.Privileges ?? Array.Empty<string>())
            .Contains(priv) ?? false);
    }

    private void MergePage(RenderResult result, Page page)
    {
        result.Layout ??= page.Layout;
        result.Path ??= page.Path;
        result.PageId = page.Id;
        result.AppId = page.AppId;
        result.ParentId = page.ParentId;
    }

    private void MergeMeta(RenderResult result, PageInfo info)
    {
        result.Culture ??= info.CultureId;
        result.Description ??= info.Description;
        result.Keywords ??= info.Keywords;
        result.Title ??= info.Title;
    }
}
