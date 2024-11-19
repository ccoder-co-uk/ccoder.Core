using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Logging;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using Microsoft.OData.Edm;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Api.OData;

public class CoreModelBuilder : ODataModelBuilder
{
    public CoreModelBuilder() : base() { }

    public override ODataModel Build() => new()
    {
        Context = "Core",
        Description = "Core Endpoints for the platform.",
        EDMModel = BuildModel()
    };

    private IEdmModel BuildModel()
    {
        // Common stuff
        AddCommonComplextypes();
        _ = Builder.ComplexType<RenderResult>();

        // Register CRUD Supporting object sets
        // CMS stuff
        _ = AddSet<App, int>();
        _ = AddSet<Layout, int>();
        _ = AddSet<Template, int>();
        _ = AddSet<Page, int>();
        _ = AddSet<PageInfo, int>();
        _ = AddSet<Content, int>();
        _ = AddSet<Component, int>();
        _ = AddSet<CommonObject, int>();
        _ = AddSet<Script, int>();
        _ = AddSet<MetaItem, int>();
        _ = AddSet<Resource, int>();
        _ = AddSet<Submission, Guid>();
        _ = AddSet<Culture, string>();

        // Security
        _ = AddSet<User, string>();
        _ = AddSet<Role, Guid>();
        _ = AddSet<Privilege, string>();

        _ = AddJoinSet<AppCulture, object>(i => new { i.AppId, i.CultureId });
        _ = AddJoinSet<UserRole, object>(i => new { i.UserId, i.RoleId });
        _ = AddJoinSet<PageRole, object>(i => new { i.PageId, i.RoleId });
        _ = AddJoinSet<FolderRole, object>(i => new { i.FolderId, i.RoleId });

        // Packaging
        _ = AddSet<Package, Guid>();
        _ = AddSet<PackageItem, Guid>();

        // forms


        // DMS stuff
        _ = AddSet<File, Guid>();
        _ = AddSet<Folder, Guid>();
        _ = AddSet<FileContent, Guid>();

        // logging stuff
        _ = AddSet<LogEntry, int>();
        _ = AddSet<LogDataItem, int>();

        // workflow stuff
        _ = AddSet<WorkflowEvent, Guid>();
        _ = AddSet<FlowDefinition, Guid>();
        _ = AddSet<FlowInstanceData, Guid>();

        // other stuff
        _ = AddSet<Calendar, int>();
        _ = AddSet<CalendarEvent, int>();
        _ = AddSet<ScheduledTask, int>();
        _ = AddSet<MailServer, int>();
        _ = AddSet<QueuedEmail, int>();
        _ = AddSet<SentEmail, int>();

        Builder.Namespace = "";

        // packaging
        _ = Builder.EntityType<Package>().Collection.Action("Import");
        _ = Builder.EntityType<Package>().Collection.Action("ImportThis");

        // App Functions and actions 
        _ = Builder.EntityType<App>().Function("Users").ReturnsFromEntitySet<User>("User");
        _ = Builder.EntityType<App>().Action("UpdatePageOrder").Parameter<App>("app");
        _ = Builder.EntityType<App>().Function("IsAdmin").Returns<bool>();
        _ = Builder.EntityType<App>().Function("Export").ReturnsCollectionFromEntitySet<Package>("Package");

        _ = Builder.EntityType<Folder>().Collection.Action("Copy").ReturnsCollection<Result<Guid?>>();
        // Page management
        _ = Builder.EntityType<Page>().Action("AddContent").Parameter<Content>("content");
        _ = Builder.EntityType<Page>().Function("RootFor").ReturnsFromEntitySet<Page>("Page");
        _ = Builder.EntityType<Page>().Function("Menu").Returns<Result<string>>();
        _ = Builder.EntityType<Page>().Collection.Function("Render").Returns<RenderResult>();

        // User and Role Functions
        _ = Builder.EntityType<User>().Collection.Function("Me").ReturnsFromEntitySet<User>("User");

        // Resourcing
        _ = Builder.EntityType<Resource>().Collection.Function("GetAll").ReturnsCollectionFromEntitySet<Resource>("Resource");

        // Component Actions
        _ = Builder.EntityType<Component>().Collection.Function("Render").Returns<string>();

        // Templating
        _ = Builder.EntityType<Template>().Collection.Action("Render").Returns<string>();

        // Workflow
        _ = Builder.EntityType<FlowDefinition>().Collection.Function("KnownActivityTypes").Returns<MetadataContainerSet>();
        _ = Builder.EntityType<FlowDefinition>().Collection.Function("KnownSystemTypes").Returns<MetadataContainerSet[]>();
        _ = Builder.EntityType<FlowInstanceData>().Action("Raw");
        _ = Builder.EntityType<FlowDefinition>().Action("Execute").Returns<Guid>();
        _ = Builder.EntityType<FlowDefinition>().Collection.Action("ExecuteScript").Returns<string>();

        //Planning
        _ = Builder.EntityType<ScheduledTask>().Action("Execute");

        //CommonObject
        _ = Builder.EntityType<CommonObject>().Collection.Function("Latest").ReturnsFromEntitySet<CommonObject>("CommonObject");
        _ = Builder.EntityType<CommonObject>().Collection.Action("Import").ReturnsCollectionFromEntitySet<Result<CommonObject>>("ImportCommonObjectResults");

        return Builder.GetEdmModel();
    }
}