using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Logging;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Data;

// Abstract types should not have constructors
// required in order to specify the minium requirements for the inherited types in the tree.
public abstract class EFDataContext<TUser, TRole> : DbContext, IDataContext<TUser>
    where TUser : class
    where TRole : class
{
    public bool FilteringEnabled { get; private set; } = true;

    public ICoreAuthInfo AuthInfo { get; set; }

    public abstract TUser User { get; }

    public Guid EventId { get; private set; }

    // Logging
    public virtual DbSet<LogEntry> Logs { get; set; }
    public virtual DbSet<LogDataItem> LogData { get; set; }

    // Security
    public virtual DbSet<TUser> Users { get; set; }
    public virtual DbSet<TRole> Roles { get; set; }

    protected Config Config { get; }

    public EventManager EventManager { get; protected set; }

    private readonly ILogger log;

    protected EFDataContext(ICoreAuthInfo auth, Config config, ILogger log)
    {
        AuthInfo = auth;
        Config = config;
        this.log = log;
    }

    public virtual void SetAuth(ICoreAuthInfo auth) => AuthInfo = auth;

    public void Migrate()
    {
        try
        {
            log.LogInformation("Migrating " + GetType().Name.Replace("DataContext", ""));
            IEnumerable<string> migrations = Database.GetPendingMigrations();

            log.LogInformation(migrations.Any()
                ? $"Pending migration about to be applied:\n\t {string.Join("\n\t", Database.GetPendingMigrations())}"
                : "No pending migrations");

            Database.Migrate();
            log.LogInformation("Migration complete");
        }
        catch (Exception ex)
        {
            log.LogInformation($"Migration of the {GetType().Name.Replace("DataContext", "")} database failed");
            log.LogError($"{ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    public virtual async Task<T> AddAsync<T>(T entity) where T : class
    {
        T result = (await base.AddAsync(entity)).Entity;
        _ = await SaveChangesAsync();

        return result;
    }

    public virtual async Task<IEnumerable<T>> AddAllAsync<T>(IEnumerable<T> items) where T : class
    {
        if (items != null && items.Any())
        {
            await base.AddRangeAsync(items);
            _ = await SaveChangesAsync();
        }

        return items;
    }

    public async Task<int> DeleteAsync<T>(T entity) where T : class
    {
        base.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task DeleteAllAsync<T>(IEnumerable<T> items) where T : class
    {
        if (items != null && items.Any())
        {
            Set<T>().RemoveRange(items);
            _ = await SaveChangesAsync();
        }
    }

    public IQueryable<T> GetAll<T>() where T : class => GetAll<T>(true);

    public virtual IQueryable<T> GetAll<T>(bool trackChanges) where T : class
    {
        IQueryable<T> result = FilteringEnabled ? Set<T>() : Set<T>().IgnoreQueryFilters();
        return trackChanges ? result : result.AsNoTracking();
    }

    public T Get<T>(object id) where T : class => GetAll<T>(true).FirstOrDefault(typeof(T).IdEquals<T>(id));

    public async Task<T> UpdateAsync<T>(T entity) where T : class
    {
        T result = base.Update(entity).Entity;
        _ = await SaveChangesAsync();
        return result;
    }

    public virtual async Task<IEnumerable<T>> UpdateAllAsync<T>(IEnumerable<T> items) where T : class
    {
        T[] results = items.Select(i => base.Update(i).Entity).ToArray();
        _ = await SaveChangesAsync();
        return results;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync();

    public virtual async Task<int> SaveChangesAsync()
    {
        try
        {
            return await SaveChangesInternal();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            log.LogWarning($"Concurrency exception raised while saving changes to the data base:\n");

            foreach (EntityEntry e in ex.Entries)
                log.LogWarning(e.Entity.ToJson(1));

            throw;
        }
        catch (Exception ex)
        {
            log.LogError($"{ex.Message}\n{ex.StackTrace}");
            Exception actualException = ex;

            while (actualException.InnerException != null)
            {
                actualException = actualException.InnerException;
                log.LogError($"{actualException.Message}\n{actualException.StackTrace}");
            }

            throw actualException;
        }
    }

    private async Task<int> SaveChangesInternal()
    {
        object[] allAdds = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToArray();

        object[] allUpdates = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToArray();

        object[] allDeletes = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToArray();


        allUpdates.ForEach(entity => BeforeUpdateEntity(entity));
        allAdds.ForEach(entity => BeforeAddEntity(entity));

        int result = 0;

        await base.SaveChangesAsync();
        await RaiseEvents(allAdds, allUpdates, allDeletes);
        return result;
    }

    private void BeforeAddEntity(object entity)
    {
        Type type = entity.GetType();
        System.Reflection.PropertyInfo createdBy = type.GetProperties().FirstOrDefault(p => p.Name == "CreatedById") ?? type.GetProperties().FirstOrDefault(p => p.Name == "CreatedBy" && p.PropertyType != typeof(TUser));
        System.Reflection.PropertyInfo created = type.GetProperties().FirstOrDefault(p => p.Name == "CreationDate") ?? (type.GetProperties().FirstOrDefault(p => p.Name == "Created")) ?? type.GetProperties().FirstOrDefault(p => p.Name == "CreatedOn");
        System.Reflection.PropertyInfo lastUpdated = type.GetProperties().FirstOrDefault(p => p.Name == "LastUpdated");
        System.Reflection.PropertyInfo lastUpdatedBy = type.GetProperties().FirstOrDefault(p => p.Name == "UpdatedById") ?? type.GetProperties().FirstOrDefault(p => p.Name == "LastUpdatedBy" && p.PropertyType != typeof(TUser));
        System.Reflection.PropertyInfo isActive = type.GetProperties().FirstOrDefault(p => p.Name == "IsActive");

        if (createdBy != null)
            createdBy.SetValue(entity, User.GetId());

        if (created != null)
            created.SetValue(entity, DateTimeOffset.UtcNow);

        if (lastUpdatedBy != null)
            lastUpdatedBy.SetValue(entity, User.GetId());

        if (lastUpdated != null)
            lastUpdated.SetValue(entity, DateTimeOffset.UtcNow);

        if (isActive != null)
            isActive.SetValue(entity, true);
    }

    private void BeforeUpdateEntity(object entity)
    {
        Type type = entity.GetType();
        System.Reflection.PropertyInfo lastUpdatedBy = type.GetProperties().FirstOrDefault(p => p.Name == "UpdatedById") ?? type.GetProperties().FirstOrDefault(p => p.Name == "LastUpdatedBy" && p.PropertyType != typeof(TUser));
        System.Reflection.PropertyInfo lastUpdated = type.GetProperties().FirstOrDefault(p => p.Name == "LastUpdated");
        System.Reflection.PropertyInfo isActive = type.GetProperties().FirstOrDefault(p => p.Name == "IsActive");
        
        if (lastUpdatedBy != null)
            lastUpdatedBy.SetValue(entity, User.GetId());

        if (lastUpdated != null)
            lastUpdated.SetValue(entity, DateTimeOffset.UtcNow);

        if (isActive != null)
            isActive.SetValue(entity, true);

        if (entity is FlowInstanceData instance)
            log.LogWarning($"Updating Instance {instance.Id} State to {instance.State}");
    }

    private async Task RaiseEvents(object[] allAdds, object[] allUpdates, object[] allDeletes)
    {
        if (EventManager != null)
        {
            await EventManager.RaiseEvents(allAdds, "Created");
            await EventManager.RaiseEvents(allUpdates, "Updated");
            await EventManager.RaiseEvents(allDeletes, "Deleted");
        }
    }

    public void DisableFilters() => FilteringEnabled = false;

    public void EnableFilters() => FilteringEnabled = true;
}