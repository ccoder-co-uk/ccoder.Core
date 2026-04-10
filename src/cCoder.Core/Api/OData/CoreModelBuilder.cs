using cCoder.Core.Models;
using cCoder.Core.Models.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using AppCulture = cCoder.Data.Models.CMS.AppCulture;
using Calendar = cCoder.Data.Models.Planning.Calendar;
using CalendarEvent = cCoder.Data.Models.Planning.CalendarEvent;
using CommonObject = cCoder.Data.Models.CommonObject;
using Component = cCoder.Data.Models.CMS.Component;
using Content = cCoder.Data.Models.CMS.Content;
using CoreApp = cCoder.Core.Models.App;
using Culture = cCoder.Data.Models.CMS.Culture;
using File = cCoder.Data.Models.DMS.File;
using FileContent = cCoder.Data.Models.DMS.FileContent;
using FlowDefinition = cCoder.Data.Models.Workflow.FlowDefinition;
using FlowInstanceData = cCoder.Data.Models.Workflow.FlowInstanceData;
using Folder = cCoder.Data.Models.DMS.Folder;
using FolderRole = cCoder.Data.Models.Security.FolderRole;
using Layout = cCoder.Data.Models.CMS.Layout;
using LogDataItem = cCoder.Data.Models.Logging.LogDataItem;
using LogEntry = cCoder.Data.Models.Logging.LogEntry;
using MailServer = cCoder.Data.Models.Mail.MailServer;
using MetaItem = cCoder.Data.Models.CMS.MetaItem;
using Package = cCoder.Data.Models.Packaging.Package;
using PackageItem = cCoder.Data.Models.Packaging.PackageItem;
using Page = cCoder.Data.Models.CMS.Page;
using PageInfo = cCoder.Data.Models.CMS.PageInfo;
using PageRole = cCoder.Data.Models.Security.PageRole;
using Privilege = cCoder.Data.Models.Security.Privilege;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using RenderResult = cCoder.ContentManagement.Models.RenderResult;
using Resource = cCoder.Data.Models.CMS.Resource;
using Role = cCoder.Data.Models.Security.Role;
using ScheduledTask = cCoder.Data.Models.Planning.ScheduledTask;
using Script = cCoder.Data.Models.CMS.Script;
using SentEmail = cCoder.Data.Models.Mail.SentEmail;
using Submission = cCoder.Data.Models.CMS.Submission;
using Template = cCoder.Data.Models.CMS.Template;
using User = cCoder.Data.Models.Security.User;
using UserRole = cCoder.Data.Models.Security.UserRole;
using WorkflowEvent = cCoder.Data.Models.Workflow.WorkflowEvent;


namespace cCoder.Core.Api.OData;

public class CoreModelBuilder : ODataModelBuilder
{
    public CoreModelBuilder()
        : base() { }

    public override ODataModel Build() =>
        new()
        {
            Context = "Core",
            Description = "Core Endpoints for the platform.",
            EDMModel = BuildModel(),
        };

    private IEdmModel BuildModel()
    {
        // Common stuff
        AddCommonComplextypes();
        _ = Builder.ComplexType<RenderResult>();
        Builder.EntityType<CoreApp>().Ignore(i => i.Config);
        Builder.EntityType<Submission>().Ignore(i => i.Data);
        Builder.EntityType<FlowInstanceData>().Ignore(i => i.ContextJson);

        // Register CRUD Supporting object sets
        // CMS stuff
        _ = AddSet<CoreApp, int>(setName: "App");
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

        _ = Builder
            .EntityType<Folder>()
            .Collection.Action("Copy")
            .ReturnsCollection<Result<Guid?>>();
        // Page management
        _ = Builder.EntityType<Page>().Action("AddContent").Parameter<Content>("content");
        _ = Builder.EntityType<Page>().Function("RootFor").ReturnsFromEntitySet<Page>("Page");
        _ = Builder.EntityType<Page>().Function("Menu").Returns<Result<string>>();
        _ = Builder.EntityType<Page>().Collection.Function("Render").Returns<RenderResult>();

        // User and Role Functions
        _ = Builder.EntityType<User>().Collection.Function("Me").ReturnsFromEntitySet<User>("User");

        // Resourcing
        _ = Builder
            .EntityType<Resource>()
            .Collection.Function("GetAll")
            .ReturnsCollectionFromEntitySet<Resource>("Resource");

        // Component Actions
        _ = Builder.EntityType<Component>().Collection.Function("Render").Returns<string>();

        // Templating
        _ = Builder.EntityType<Template>().Collection.Action("Render").Returns<string>();
        _ = Builder
            .EntityType<Template>()
            .Collection.Action("HtmlToPdf")
            .Returns<FileContentResult>();

        // Workflow
        _ = Builder
            .EntityType<FlowDefinition>()
            .Collection.Function("KnownActivityTypes")
            .Returns<MetadataContainerSet>();
        _ = Builder
            .EntityType<FlowDefinition>()
            .Collection.Function("KnownSystemTypes")
            .Returns<MetadataContainerSet[]>();
        _ = Builder.EntityType<FlowInstanceData>().Action("Raw");
        _ = Builder.EntityType<FlowDefinition>().Action("Execute").Returns<Guid>();
        _ = Builder
            .EntityType<FlowDefinition>()
            .Collection.Action("ExecuteScript")
            .Returns<string>();

        //Planning
        _ = Builder.EntityType<ScheduledTask>().Action("Execute");

        //CommonObject
        _ = Builder
            .EntityType<CommonObject>()
            .Collection.Function("Latest")
            .ReturnsFromEntitySet<CommonObject>("CommonObject");
        _ = Builder
            .EntityType<CommonObject>()
            .Collection.Action("Import")
            .ReturnsCollectionFromEntitySet<Result<CommonObject>>("ImportCommonObjectResults");

        return Builder.GetEdmModel();
    }
}








