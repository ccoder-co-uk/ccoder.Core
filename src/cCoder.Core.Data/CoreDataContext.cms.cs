using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Security;

namespace cCoder.Core.Data;

public partial class CoreDataContext
{
    // Content Management
    public virtual DbSet<Layout> Layouts { get; set; }
    public virtual DbSet<App> Apps { get; set; }
    public virtual DbSet<Page> Pages { get; set; }
    public virtual DbSet<PageInfo> PageInfo { get; set; }
    public virtual DbSet<Content> Contents { get; set; }
    public virtual DbSet<Component> Components { get; set; }
    public virtual DbSet<Resource> Resources { get; set; }
    public virtual DbSet<Culture> Cultures { get; set; }
    public virtual DbSet<Template> Templates { get; set; }
    public virtual DbSet<Submission> Submissions { get; set; }
    public virtual DbSet<Script> Scripts { get; set; }
    public virtual DbSet<CommonObject> CommonObjects { get; set; }

    // Join Entities
    public virtual DbSet<AppCulture> AppCultures { get; set; }
    public virtual DbSet<PageRole> PageRoles { get; set; }

    public int? GetAppId<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity == null)
            return null;

        Type type = entity.GetType();

        int? result = null;
        System.Reflection.PropertyInfo appIdProp = type.GetProperty("AppId");
        if (appIdProp != null)
            result = (int)appIdProp.GetValue(entity);

        //ok this is where things get tough this entity has no appId so we have to attempt to crawl the tree ...
        // first we look for a parent attrib:
        else if (Attribute.GetCustomAttributes(type, typeof(ParentAttribute)).FirstOrDefault() is ParentAttribute parentPropertyRef)
        {
            // once found we can ask the parent entity the same question:
            object parent = type.GetProperty(parentPropertyRef.PropertyName).GetValue(entity);
            if (parent == null)
            {
                // unless it hasn't been loaded, so lets dynamically figure that out and load it:
                TEntity eWithParent = GetAll<TEntity>(false)
                    .Include(parentPropertyRef.PropertyName)
                    .FirstOrDefault(typeof(TEntity).IdEquals<TEntity>(entity.GetId()));

                //Despite being told not to track changes directly above... EF decides it should track it... Potential bug in EF? Perhaps to do with includes or the dynamic interferring with calls.
                if (eWithParent != null)
                {
                    ChangeTracker.Entries<TEntity>().First(i => i.Entity.GetId().ToString() == eWithParent.GetId().ToString()).State = EntityState.Detached;
                    parent = type.GetProperty(parentPropertyRef.PropertyName)?.GetValue(eWithParent);
                }
            }

            // if this returns null, we may have bad data, all we can do is run with it and hope the parent scope knows what to do.
            result = GetAppId(parent);
        }

        return result;
    }

    public void DeleteApp(int appId)
    {
        if (User?.IsAdminOfApp(appId) != true)
            throw new SecurityException("Access Denied!");

        Database.SetCommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
        _ = Database.ExecuteSqlRaw(@"
-- security related rows 
DELETE FROM [Security].FolderRoles WHERE RoleId IN (SELECT Id FROM [Security].Roles WHERE AppId = @p0)
DELETE FROM [Security].PageRoles WHERE RoleId IN (SELECT Id FROM [Security].Roles WHERE AppId = @p0)
DELETE FROM [Security].UserRoles WHERE RoleId IN (SELECT Id FROM [Security].Roles WHERE AppId = @p0)
DELETE FROM [Security].Roles WHERE AppId = @p0

---- DMS related rows
DELETE FROM DMS.[FileContents] WHERE Id in ( SELECT fc.Id FROM DMS.[FileContents] fc JOIN DMS.Files f ON fc.FileId = f.Id JOIN DMS.Folders fol ON fol.Id = f.FolderId WHERE fol.AppId = @p0 )
DELETE FROM DMS.Files WHERE Id in ( SELECT f.Id FROM DMS.Files f JOIN DMS.Folders fol ON fol.Id = f.FolderId WHERE fol.AppId = @p0 )
DELETE FROM DMS.Folders WHERE Id in ( SELECT Id FROM DMS.Folders WHERE AppId = @p0 )

---- CMS related rows
DELETE FROM CMS.Components WHERE AppId = @p0
DELETE FROM CMS.PageInfo WHERE PageId in (SELECT Id FROM CMS.Pages WHERE AppId = @p0)
DELETE FROM CMS.Contents WHERE PageId in (SELECT Id FROM CMS.Pages WHERE AppId = @p0)
DELETE FROM CMS.Pages WHERE AppId = @p0
DELETE FROM CMS.Layouts WHERE AppId = @p0
DELETE FROM CMS.Resources WHERE AppId = @p0
DELETE FROM CMS.Templates WHERE AppId = @p0
DELETE FROM CMS.AppCultures WHERE AppId = @p0

---- Planning Items
DELETE FROM Planning.BackgroundJobs WHERE AppId = @p0
DELETE FROM Mail.MailServers WHERE AppId = @p0
DELETE FROM Mail.SentEmails WHERE AppId = @p0
DELETE FROM Planning.Events WHERE CalendarId in (SELECT Id FROM Planning.Calendars WHERE AppId = @p0)
DELETE FROM Planning.Calendars WHERE Id in (SELECT Id FROM Planning.Calendars WHERE AppId = @p0)
DELETE FROM Planning.ScheduledTasks WHERE AppId = @p0

---- workflow related rows 
DELETE FROM Workflow.WorkflowEvents WHERE FlowId IN (SELECT Id FROM Workflow.WorkFlows WHERE AppId = @p0)
DELETE FROM Workflow.FlowInstances WHERE FlowDefinitionId in (SELECT Id FROM Workflow.WorkFlows WHERE AppId = @p0)
DELETE FROM Workflow.WorkFlows WHERE AppId = @p0
DELETE FROM Workflow.BusinessProcesses WHERE AppId = @p0

---- Drop other items
DELETE FROM Mail.EmailSendFailures
DELETE FROM Mail.QueuedEmails WHERE AppId = @p0
DELETE FROM Mail.SentEmails WHERE AppId = @p0
DELETE FROM CMS.Submissions WHERE AppId = @p0

---- and finally the app
DELETE FROM CMS.Apps WHERE Id = @p0
        ",
                appId);
    }

    public async Task DeletePage(int pageId)
    {
        try
        {
            DisableFilters();
            Page page = GetAll<Page>(true)
                .Include(p => p.PageInfo)
                .Include(p => p.Contents)
                .Include(p => p.App)
                .Include(p => p.Pages)
                .Include(p => p.Roles)
                .FirstOrDefault(p => p.Id == pageId);

            if (page != null && (User.IsAdminOfApp(page.AppId) || page.Roles.Any(r => User.Roles.Any(ur => r.RoleId == ur.RoleId) && r.Role.Privileges.Contains("page_delete"))))
            {
                // drop child pages (recursively)
                if (page.Pages != null && page.Pages.Any())
                {
                    await Task.WhenAll(page.Pages.Select(p => DeletePage(p.Id)));
                }

                // drop the page
                await DeleteAllAsync(page.PageInfo);
                await DeleteAllAsync(page.Contents);
                await DeleteAllAsync(page.Roles);
                _ = await DeleteAsync(page);
                _ = await SaveChangesAsync();
            }
            else
            {
                throw new SecurityException("Access Denied!");
            }
        }
        catch (Exception ex)
        {
            log.LogDebug("Failed To Delete Page Due To Exception : " + ex.Message + "\n" + ex.StackTrace);
            throw;
        }
        finally
        {
            EnableFilters();
        }
    }

    public IEnumerable<Resource> GetResourcesBy(int appId, string key, string culture)
    {
        List<Resource> results = new();
        List<IGrouping<string, Resource>> potentials = GetAll<Resource>()
            .Where(r => r.Key.ToLower() == key.ToLower() && r.AppId == appId)
            .AsEnumerable<Resource>()
            .GroupBy(r => r.Name)
            .ToList();

        foreach (IGrouping<string, Resource> resGroup in potentials)
        {
            results.Add(GetClosestCulturalMatch(resGroup, culture));
        }

        return results.Where(r => r != null).ToList();
    }

    /// <summary>
    /// Implements the sub culture fallback match logic on sets of resources with the same key to ensure we find the best / closest match when a specific match cannot be found
    /// </summary>
    /// <param name="potentials">the set of resources that share a common key</param>
    /// <param name="culture">the cultural preference to find the closest match to</param>
    /// <returns></returns>
    private static Resource GetClosestCulturalMatch(IEnumerable<Resource> potentials, string culture)
    {
        Resource result = null;
        List<string> cultureParts = culture.ToLower().Split('-').ToList();
        int take = cultureParts.Count;
        string resultCulture = "";

        // scan the cultural heirarchy in the code
        while (result == null && resultCulture != null)
        {
            resultCulture = string.Join("-", cultureParts.Take(take));
            result = potentials.FirstOrDefault(r => r.Culture.ToLowerInvariant() == resultCulture.ToLowerInvariant());
            take--;
            if (take == 0)
                resultCulture = null;
        }

        if (result == null)
            result = potentials.FirstOrDefault(r => string.IsNullOrEmpty(r.Culture.ToLowerInvariant()) || r.Culture == null);

        return result;
    }
}
