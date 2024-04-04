using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Services
{
    public interface IOrchestrationService<T>
    {
        Task Run(T job, ICoreAuthInfo authInfo);
    }

    /// <summary>
    /// The root of all business logic providing types
    /// </summary>
    public interface IService : IDisposable
    {
        ICoreAuthInfo AuthInfo { get; }

        void SetAuth(ICoreAuthInfo auth);
    }

    /// <summary>
    /// Core service definition
    /// </summary>
    public interface IService<T, TUser> : IReadOnlyService<T>, IWriteableService<T>, IService
        where T : class
    {
        TUser User { get; }
    }

    /// <summary>
    /// Defines the base querying interface for services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyService<out T> where T : class
    {
        /// <summary>
        /// Gets the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>the object of type T with the given Id</returns>
        T Get(object id);

        /// <summary>
        /// Gets the whole set of T
        /// </summary>
        /// <returns></returns>
        IQueryable<T> GetAll();

        /// <summary>
        ///  Gets the whole set of T with change tracking preference
        /// </summary>
        /// <param name="andTrack"></param>
        /// <returns></returns>
        IQueryable<T> GetAll(bool andTrack);
    }

    /// <summary>
    /// Defines the base command interface for all services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWriteableService<T> where T : class
    {
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        Task DeleteAsync(object id);

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Adds all of the given items to the entity set in the repository
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<IEnumerable<Result<T>>> AddAllAsync(IEnumerable<T> items);

        /// <summary>
        /// Adds all of the given items to the entity set in the repository
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<IEnumerable<Result<T>>> UpdateAllAsync(IEnumerable<T> items);

        Task<IEnumerable<Result<T>>> AddOrUpdate(IEnumerable<T> items, bool onlyIfNewer = true);

        /// <summary>
        /// Deletes all of the given items in the entity set in the repository
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task DeleteAllAsync(IEnumerable<T> items);
    }

    public interface ITemplateService : ICoreService<Template>
    {
        public Task<string> Render(int appId, string name, string culture, dynamic model);
    }

    /// <summary>
    /// Core services base definition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoreService<T> : IService<T, User> where T : class { }

    public interface IUserRoleService : ICoreService<UserRole>
    {
        Task<UserRole> SaveAsync(UserRole entity);
    }

    public interface IQueuedEmailService : ICoreService<QueuedEmail>
    {
        Task<QueuedEmail> AddAsync(QueuedEmail entity, bool checkPrivs);
    }

    public interface IPackageService : ICoreService<Package>
    {
        Task Import(int appId, Package package);
        Task Import(int appId, string packageUrl, string remoteAuth);
    }

    public interface IFlowDefinitionService : ICoreService<FlowDefinition>
    {
        Task<Guid> Queue(Guid id, string args);
    }

    public interface IScheduledTaskService : ICoreService<ScheduledTask>
    {
        Task Execute(int id, bool incrementNextExecution = true);
    }

    public interface IComponentService : ICoreService<Component>
    {
        string Render(int appId, string name, string culture, string theme);
    }

    public interface IPageService : ICoreService<Page>
    {
        Task RecomputeAllForAppAsync(int appId);
        RenderResult Render(int appId, string path, string theme, string culture, bool edit = false);
        Page GetRoot(int id);
        IEnumerable<Page> GetChildren(int id);
        string MenuFor(int id, string culture);
    }

    public interface IAppService : ICoreService<App>
    {
        IQueryable<User> GetAppUsers(int appId);
        IEnumerable<Package> Export(int appId, string[] packages);
        Task Import(string name, string domain, Stream packageStream);
        Task UpdatePageOrder(int key, App app);
        bool IsAdmin(int appId, string userName);
    }

    public interface ICommonObjectService : ICoreService<CommonObject>
    {
        IEnumerable<CommonObject> Latest(string type);
        Task<IEnumerable<Result<CommonObject>>> Import(IEnumerable<CommonObject> items);
    }
    public interface IScriptService : ICoreService<Script> { }

    public interface IResourceService : ICoreService<Resource>
    {
        IEnumerable<Resource> GetAll(string key, string culture, int appId);
    }

    public interface IFolderService : ICoreService<Folder>
    {
        Task<List<Result<Guid?>>> Copy(string source, string destination, int sourceAppId, int destAppId);
    }

    /// <summary>
    /// File handling service
    /// </summary>
    public interface IFileService : ICoreService<Objects.Entities.DMS.File>
    {
        Objects.Entities.DMS.File GetByPath(int appId, string path);
    }

}