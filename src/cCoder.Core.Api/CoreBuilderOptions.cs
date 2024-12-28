using cCoder.Core.Data;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using cCoder.Core.Services.DMS;
using cCoder.Core.Services.EventHandlers;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;
using cCoder.Core.Services.Events.DMS_Moves;
using iText.Commons.Actions;
using Microsoft.OData.Edm;
using cCoder.Core.Services.EventHandlers.DMS_Moves;
using cCoder.Core.Services.Events;
using cCoder.Core.Objects.Entities.DMS;

namespace cCoder.Core.Api;

public class CoreBuilderOptions
{
    private readonly IServiceCollection services;

    public CoreBuilderOptions(IServiceCollection services)
    {
        this.services = services;
    }

    public CoreBuilderOptions UseMSSQLProvider(string connectionString)
    {
        //services.AddDbContext<CoreDataContext>();
        services.AddScoped<ICoreDataContext, CoreDataContext>();

        services.AddDbContextFactory<CoreDataContext>(lifetime: ServiceLifetime.Scoped);

        return this;
    }

    public CoreBuilderOptions UseContentManagement(IDictionary<string, IEdmModel> map = null, bool servicesOnly = false)
    {
        services.AddCoreServices();
        services.AddCorePackaging();
        services.AddCaches(map ?? new Dictionary<string, IEdmModel>());

        if (!servicesOnly)
            services.AddAspNet();

        return this;
    }

    public CoreBuilderOptions UseDocumentManagement()
    {
        services.AddScoped<IEventService, EventService>();

        services.AddScoped<IEventHandler<FileMovedToExistingFolderEvent, FileMovedToExistingFolderVO>, FileMovedToExistingFolderEventHandler>();
        services.AddScoped<IEventHandler<FileMovedToExistingFileEvent, FileMovedToExistingFileVO>, FileMovedToExistingFileEventHandler>();

        services.AddScoped<IEventHandler<FileUpdatedEvent, Objects.Entities.DMS.File>, FileUpdatedEventHandler>();
        services.AddScoped<IEventHandler<FileDeletedEvent, Objects.Entities.DMS.File>, FileDeletedEventHandler>();

        services.AddScoped<IEventHandler<FolderMovedToExistingFolderEvent, FolderMovedToExistingFolderVO>, FolderMovedToExistingFolderEventHandler>();
        services.AddScoped<IEventHandler<FolderMovedToNewFolderEvent, FolderMovedToNewFolderVO>, FolderMovedToNewFolderEventHandler>();

        services.AddScoped<IEventHandler<FolderCreatedEvent, Folder>, FolderCreatedEventHandler>();
        services.AddScoped<IEventHandler<FolderUpdatedEvent, Folder>, FolderUpdatedEventHandler>();
        services.AddScoped<IEventHandler<FolderDeletedEvent, Folder>, FolderDeletedEventHandler>();

        // DMS service
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IFolderService, FolderService>();
        services.AddTransient<ICoreService<FolderRole>, FolderRoleService>();

        return this;
    }

    public CoreBuilderOptions AuthorizeUsersWith(Func<IServiceProvider, string> authResolver)
    {
        services.AddTransient<ICoreAuthInfo>(ctx => new CoreAuthInfo { SSOUserId = authResolver(ctx) });
        return this;
    }
}

public class CoreAuthInfo : ICoreAuthInfo
{
    public string SSOUserId { get; internal set; }
}