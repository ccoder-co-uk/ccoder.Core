using Core.Objects.Dtos.Metadata;
using Core.Objects.Dtos.Workflow;
using Core.Objects.Entities;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Packaging;
using Core.Objects.Entities.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Objects
{
    public delegate Task LogEvent(WorkflowLogLevel level, string message);


    public interface IEventBusProvider : IDisposable
    {
        event Func<QueueMessage, Task> ProcessMessageAsync;
        event Func<Exception, Task> ProcessErrorAsync;
        Task StartProcessingAsync();
    }

    public interface IMetadataCache : ICache<MetadataContainerSet>
    {
        void Rebuild();

        string GetAll(string culture = "");
        string Get(string key, string culture);
        void Set(string key, string value, string culture);
        string ToJson(string culture);
    }

    public interface ICommonObjectCache : ICache<object>
    {
        void Refresh();
        T[] GetAll<T>();
        T Get<T>(string key);
        IEnumerable<CommonObject> LatestSet { get; set; }
    }

    public interface IPackageInstaller
    {
        string Type { get; set; }
        Task Import(int appId, Package package);
    }

    public interface IPackageItemImporter
    {
        string Type { get; }

        int Order { get; }

        Task Import(int appId, PackageItem item);
    }

    public interface IAmRoleSecured<TRole>
    {
        ICollection<TRole> Roles { get; set; }

        bool UserCan(User user, string priv);
    }

    public interface ICoreAuthInfo
    {
        string SSOUserId { get; }
    }

    public interface IResourceProvider : IDisposable
    {
        Resource GetResource(string key, string culture);
    }

    public interface ICache<T> : IDisposable
    {
        int ExpiryTimeInMinutes { get; set; }
        T Get(string key);
        void Set(string key, T item);
        IDictionary<string, T> ToDictionary();
    }

    public interface ICrypto<T>
    {
        string Encrypt(T source, string key);
        string Encrypt(T source);

        T Decrypt(string source, string key);
        T Decrypt(string source);
    }

    public interface IWorkflowContext
    {
        IScriptRunner Script { get; }

        IDictionary<string, object> Variables { get; }

        public Flow Flow { get; }

        void Log(WorkflowLogLevel level, string message);
    }

    public interface IScriptRunner
    {
        Task Run(string code, string[] imports, object args, Action<WorkflowLogLevel, string> log);
        Task<T> Run<T>(string code, string[] imports, object args = null, Action<WorkflowLogLevel, string> log = null);
        Task<T> BuildScript<T>(string code, string[] imports, Action<WorkflowLogLevel, string> log);
    }

}