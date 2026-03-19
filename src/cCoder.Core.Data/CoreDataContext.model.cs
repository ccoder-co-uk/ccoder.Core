using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Data;

public partial class CoreDataContext
{
    private static readonly string[] sysMethods = new[] { "ToString", "Equals", "GetHashCode", "GetType" };

    private IEnumerable<int> AdminOf => User.Roles?.Where(r => r.Role.Privileges.Any(p => p == "app_admin")).Select(r => r.Role.AppId) ?? Array.Empty<int>();

    private IEnumerable<Guid> CurrentUserRoleIds => User.Roles?.Select(r => r.RoleId) ?? Array.Empty<Guid>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
        ApplyFilters(modelBuilder);
        Seed(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureModel(ModelBuilder builder)
    {
        _ = builder.Entity<Role>().Ignore(r => r.Privileges);
        _ = builder.Entity<App>().Ignore(i => i.Config);
        _ = builder.Ignore<Email>();
        _ = builder.Ignore<BaseEntity>();
        _ = builder.Entity<FlowInstanceData>().Ignore(r => r.ContextString);
        _ = builder.Entity<AppCulture>().HasKey(i => new { i.AppId, i.CultureId });
        _ = builder.Entity<UserRole>().HasKey(i => new { i.RoleId, i.UserId });
        _ = builder.Entity<PageRole>().HasKey(i => new { i.PageId, i.RoleId });
        _ = builder.Entity<FolderRole>().HasKey(i => new { i.FolderId, i.RoleId });

        IEnumerable<global::Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey> cascadingRelationships = builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (global::Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey relationship in cascadingRelationships)
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }

    private void ApplyFilters(ModelBuilder builder)
    {
        // DMS security
        _ = builder.Entity<FolderRole>().HasQueryFilter(fr => fr.Role != null);
        _ = builder.Entity<Folder>().HasQueryFilter(f => f.DeletedOn == null && (AdminOf.Contains(f.AppId) || f.Roles.Any(r => CurrentUserRoleIds.Contains(r.RoleId) && r.Role.Privs.Contains("folder_read"))));
        _ = builder.Entity<File>().HasQueryFilter(f => f.DeletedOn == null && (AdminOf.Contains(f.Folder.AppId) || f.Folder.Roles.Any(r => CurrentUserRoleIds.Contains(r.RoleId) && r.Role.Privs.Contains("file_read"))));
        _ = builder.Entity<FileContent>().HasQueryFilter(i => i.File != null);
        _ = builder.Entity<WorkflowEvent>().HasQueryFilter(e => e.Flow != null);

        // CMS security
        _ = builder.Entity<Role>().HasQueryFilter(r => AdminOf.Contains(r.AppId) || CurrentUserRoleIds.Contains(r.Id));
        _ = builder.Entity<UserRole>().HasQueryFilter(ur => ur.Role != null);
        _ = builder.Entity<User>().HasQueryFilter(u => u.Roles.Any());
        _ = builder.Entity<PageRole>().HasQueryFilter(pr => pr.Role != null);
        _ = builder.Entity<Page>().HasQueryFilter(p => AdminOf.Contains(p.AppId) || p.Roles.Any(pr => CurrentUserRoleIds.Contains(pr.RoleId) && pr.Role.Privs.Contains("page_read")));
        _ = builder.Entity<PageInfo>().HasQueryFilter(i => i.Page != null);
        _ = builder.Entity<Content>().HasQueryFilter(i => i.Page != null);
        _ = builder.Entity<Submission>().HasQueryFilter(s => AdminOf.Contains(s.AppId) || s.App.Roles.Any(r => CurrentUserRoleIds.Contains(r.Id) && r.Privs.Contains("submission_read")));

        // Mail security
        _ = builder.Entity<SentEmail>().HasQueryFilter(mail => mail.SentByUserId == User.Id || AdminOf.Contains(mail.AppId));
        _ = builder.Entity<QueuedEmail>().HasQueryFilter(mail => mail.SentByUserId == User.Id || AdminOf.Contains(mail.AppId));

        // other
        _ = builder.Entity<ScheduledTask>().HasQueryFilter(t => AdminOf.Contains(t.AppId));
        _ = builder.Entity<Calendar>().HasQueryFilter(c => AdminOf.Contains(c.AppId) || c.App.Roles.Any(r => CurrentUserRoleIds.Contains(r.Id) && r.Privs.Contains("calendar_read")));
        _ = builder.Entity<CalendarEvent>().HasQueryFilter(e => e.Calendar != null);
        _ = builder.Entity<FlowDefinition>().HasQueryFilter(f => AdminOf.Contains(f.AppId) || f.App.Roles.Any(r => CurrentUserRoleIds.Contains(r.Id) && r.Privs.Contains("flowdefinition_read")));
        _ = builder.Entity<FlowInstanceData>().HasQueryFilter(f => f.FlowDefinition != null);
    }

    private void Seed(ModelBuilder builder)
    {
        _ = builder.Entity<Culture>().HasData(Data.Cultures.Known);
        _ = builder.Entity<Privilege>().HasData(ComputePrivileges());
    }

    private Privilege[] ComputePrivileges()
    {
        // get managed types 
        Type[] types = GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetInterface("IQueryable") != null)
            .Select(p => p.PropertyType.GenericTypeArguments[0])
            .ToArray();

        string suffix;

        Privilege[] privs = types.SelectMany(t =>
        {
            suffix = t.Name.EndsWith("s") ? "es" : "s";

            // create CRUD privs for t
            List<Privilege> p = new()
            {
                new Privilege() { Id = $"{t.Name}_Create", Type = t.Name, Operation = "Create", Description = $"Allows users to Create {t.Name}{suffix}." },
                new Privilege() { Id = $"{t.Name}_Read", Type = t.Name, Operation = "Read", Description = $"Allows users to Read {t.Name}{suffix}." },
                new Privilege() { Id = $"{t.Name}_Update", Type = t.Name, Operation = "Update", Description = $"Allows users to Update {t.Name}{suffix}." },
                new Privilege() { Id = $"{t.Name}_Delete", Type = t.Name, Operation = "Delete", Description = $"Allows users to Delete {t.Name}{suffix}." }
            };

            // create method call privs for t
            t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && !sysMethods.Contains(m.Name) && m.GetCustomAttribute<DontPrivilegeAttribute>() == null)
                .ForEach(m => p.Add(new Privilege() { Id = $"{t.Name}_{m.Name}", Type = t.Name, Operation = m.Name, Description = $"Allows users to call {m.Name} on {t.Name}{suffix}." }));

            return p;
        })
        .ToArray();

        privs.ForEach(p => p.Id = p.Id.ToLower());
        List<Privilege> result = privs.GroupBy(p => p.Id).Select(g => g.First()).ToList();

        result.AddRange(new[] {
            new Privilege() { Id = $"folder_updatefiles", Type = "Folder", Operation = "UpdateFiles", Description = $"Allows users to manage Files for a folder." },
            new Privilege() { Id = $"folder_updateroles", Type = "Folder", Operation = "UpdateRoles", Description = $"Allows users to manage Roles for a folder." },
            new Privilege() { Id = $"folder_updatesubfolders", Type = "Folder", Operation = "UpdateSubFolders", Description = $"Allows users to manage Sub folders for a folder." }
        });

        result.Add(new Privilege() { Id = $"app_admin", Type = "App", Operation = "Admin", Description = $"Marks users in this role as App Admins." });
        return result.ToArray();
    }
}