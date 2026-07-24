// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Models.Metadata;
using cCoder.Core.Exposures.OData;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Workflow;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Logging;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Packaging;
using cCoder.ContentManagement.Models;

namespace cCoder.Core.Dependencies.OData;

internal sealed class CoreModelBuilderDependency
    : cCoder.Core.Exposures.OData.ODataModelBuilder,
      IConfigureOptions<ODataConventionModelBuilder>
{
    public override ODataModel Build() =>
        new()
        {
            Context = "Core",
            Description = "Core Endpoints for the platform.",
            EDMModel = BuildModel(),
        };

    public void Configure(ODataConventionModelBuilder options)
    {
        AddCommonComplextypes();
        _ = options.ComplexType<RenderResult>();

        options.EntityType<App>()
            .Ignore(propertyExpression: i => i.Config);

        options.EntityType<Submission>()
            .Ignore(propertyExpression: i => i.Data);

        options.EntityType<FlowInstanceData>()
            .Ignore(propertyExpression: i => i.ContextJson);

        _ = AddSet<App, int>(setName: "App");
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

        _ = AddSet<User, string>();
        _ = AddSet<Role, Guid>();
        _ = AddSet<Privilege, string>();

        _ = AddJoinSet<AppCulture, object>(key: i => new { i.AppId, i.CultureId });
        _ = AddJoinSet<UserRole, object>(key: i => new { i.UserId, i.RoleId });
        _ = AddJoinSet<PageRole, object>(key: i => new { i.PageId, i.RoleId });
        _ = AddJoinSet<FolderRole, object>(key: i => new { i.FolderId, i.RoleId });

        _ = AddSet<Package, Guid>();
        _ = AddSet<PackageItem, Guid>();


        _ = AddSet<Data.Models.DMS.File, Guid>();
        _ = AddSet<Folder, Guid>();
        _ = AddSet<FileContent, Guid>();

        _ = AddSet<LogEntry, int>();
        _ = AddSet<LogDataItem, int>();

        _ = AddSet<WorkflowEvent, Guid>();
        _ = AddSet<FlowDefinition, Guid>();
        _ = AddSet<FlowInstanceData, Guid>();

        _ = AddSet<Calendar, int>();
        _ = AddSet<CalendarEvent, int>();
        _ = AddSet<ScheduledTask, int>();
        _ = AddSet<MailServer, int>();
        _ = AddSet<QueuedEmail, int>();
        _ = AddSet<SentEmail, int>();

        options.Namespace = "";

        _ = options.EntityType<Package>().Collection.Action(name: "Import");
        _ = options.EntityType<Package>().Collection.Action(name: "ImportThis");

        _ = options
            .EntityType<Folder>()
            .Collection.Action(name: "Copy")
            .ReturnsCollection<ContentManagement.Models.Result<Guid?>>();

        _ = options.EntityType<Page>()
            .Action(name: "AddContent")
                .Parameter<Content>(name: "content");

        _ = options.EntityType<Page>()
            .Function(name: "RootFor")
                .ReturnsFromEntitySet<Page>(entitySetName: "Page");

        _ = Builder.EntityType<Page>()
            .Function(name: "Menu")
            .Returns<ContentManagement.Models.Result<string>>();

        _ = options.EntityType<Page>().Collection.Function(name: "Render")
            .Returns<RenderResult>();

        _ = options.EntityType<User>().Collection.Function(name: "Me")
            .ReturnsFromEntitySet<User>(entitySetName: "User");

        _ = options
            .EntityType<Resource>()
            .Collection.Function(name: "GetAll")
            .ReturnsCollectionFromEntitySet<Resource>(entitySetName: "Resource");

        _ = options.EntityType<Component>().Collection.Function(name: "Render")
            .Returns<string>();

        _ = options.EntityType<Template>().Collection.Action(name: "Render")
            .Returns<string>();

        _ = options
            .EntityType<Template>()
            .Collection.Action(name: "HtmlToPdf")
            .Returns<FileContentResult>();

        _ = options
            .EntityType<FlowDefinition>()
            .Collection.Function(name: "KnownActivityTypes")
            .Returns<MetadataContainerSet>();

        _ = options
            .EntityType<FlowDefinition>()
            .Collection.Function(name: "KnownSystemTypes")
            .Returns<MetadataContainerSet[]>();

        _ = options.EntityType<FlowInstanceData>()
            .Action(name: "Raw");

        _ = options.EntityType<FlowDefinition>()
            .Action(name: "Execute")
                .Returns<Guid>();

        _ = options
            .EntityType<FlowDefinition>()
            .Collection.Action(name: "ExecuteScript")
            .Returns<string>();

        _ = options.EntityType<ScheduledTask>()
            .Action(name: "Execute");

        _ = options
            .EntityType<CommonObject>()
            .Collection.Function(name: "Latest")
            .ReturnsFromEntitySet<CommonObject>(entitySetName: "CommonObject");

        _ = options
            .EntityType<CommonObject>()
            .Collection.Action(name: "Import")
            .ReturnsCollectionFromEntitySet<ContentManagement.Models.Result<CommonObject>>(entitySetName: "ImportCommonObjectResults");

    }

    private IEdmModel BuildModel()
    {
        Configure(options: Builder);

        return Builder.GetEdmModel();
    }
}