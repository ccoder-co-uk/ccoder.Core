using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;

namespace cCoder.Core.Objects;

public interface ICmsDataContext : IDataContext<User>
{
    // content management functions 
    Task DeletePage(int pageId);
    void DeleteApp(int appId);

    // resourcing functions
    IEnumerable<Resource> GetResourcesBy(int appId, string key, string culture);

    int? GetAppId<TEntity>(TEntity entity) where TEntity : class;
}

public interface IDmsDataContext
{
    // document management functions
    Task DeleteFolder(Guid folderId);
    void DeleteFile(Guid fileId);
}

/// <summary>
/// Core data store definition
/// </summary>
public interface ICoreDataContext : ICmsDataContext, IDmsDataContext
{
    Privilege[] GetAllPrivileges();
    int FlushWFInstances(DateTimeOffset from);
}

public interface ITransaction : IDisposable
{
    void Commit();
    void Rollback();
}

/// <summary>
/// Base data context definition for all CRUD sources
/// </summary>
public interface IDataContext : IReadOnlyDataContext, IWriteableDataContext, IDisposable
{
    //Pulls read, write, and disposable functionality together.
    //Useful marker interface for later (command query pattern)
    Guid EventId { get; }
    ICoreAuthInfo AuthInfo { get; set; }

    void SetAuth(ICoreAuthInfo auth);

    void DisableFilters();
    void EnableFilters();
    void Migrate();
}

/// <summary>
/// base generic data store definition
/// </summary>
/// <typeparam name="T">repository type</typeparam>
public interface IDataContext<out TUser> : IDataContext
{
    TUser User { get; }
}

public interface IReadOnlyDataContext
{
    /// <summary>
    /// Gets the specified id.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>the object of type T with the given Id</returns>
    T Get<T>(object id) where T : class;

    /// <summary>
    /// Gets the collection of T.
    /// </summary>
    /// <returns>The set of T</returns>
    IQueryable<T> GetAll<T>() where T : class;

    /// <summary>
    /// Gets the specified id.
    /// </summary>
    /// <returns>A set of object of type T</returns>
    IQueryable<T> GetAll<T>(bool trackChanges) where T : class;
}

/// <summary>
/// Interface definining the base definition of all writable data contexts in the webstack
/// </summary>
public interface IWriteableDataContext
{
    /// <summary>
    /// Updates the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    Task<T> UpdateAsync<T>(T entity) where T : class;

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    Task<int> DeleteAsync<T>(T entity) where T : class;

    /// <summary>
    /// Adds the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns></returns>
    Task<T> AddAsync<T>(T entity) where T : class;

    /// <summary>
    /// Adds all of the specified items to the data store
    /// </summary>
    /// <typeparam name="T">type of entities being added</typeparam>
    /// <param name="items">collection of T's to be added</param>
    /// <returns>collection of T's that got added</returns>
    Task<IEnumerable<T>> AddAllAsync<T>(IEnumerable<T> items) where T : class;

    /// <summary>
    /// Updates all of the specified items to the data store
    /// </summary>
    /// <typeparam name="T">type of entities being updated</typeparam>
    /// <param name="items">collection of T's to be updated</param>
    /// <returns>collection of T's that got updated</returns>
    Task<IEnumerable<T>> UpdateAllAsync<T>(IEnumerable<T> items) where T : class;

    /// <summary>
    /// Deletes all of the specified items from the data store
    /// </summary>
    /// <typeparam name="T">type of entities being deleted</typeparam>
    /// <param name="items">collection of T's to be deleted</param>
    Task DeleteAllAsync<T>(IEnumerable<T> items) where T : class;

    /// <summary>
    /// Saves any pending changes.
    /// </summary>
    /// <param name="andAudit">Optional parameter determines if any applicable auditing should be done.</param>
    /// <param name="andUpdateComputedFields">Optional parameter determines if computed fields should be updated.</param>
    Task<int> SaveChangesAsync();
}

/// <summary>
/// Interface for use when data contexts need to be synchronously writable
/// </summary>
public interface ISynchronousWriteableDataContext
{
    /// <summary>
    /// Updates the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    T Update<T>(T entity) where T : class;

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    void Delete<T>(T entity) where T : class;

    /// <summary>
    /// Adds the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns></returns>
    T Add<T>(T entity) where T : class;

    /// <summary>
    /// Saves any pending changes.
    /// </summary>
    /// <param name="andAudit">Optional parameter determines if any applicable auditing should be done.</param>
    int SaveChanges(bool andAudit = true);
}