// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using cCoder.Eventing.Http.Models;
using FluentAssertions;
using HostedServices.AcceptanceTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AppEntity = cCoder.Data.Models.CMS.App;
using DmsFile = cCoder.Data.Models.DMS.File;

namespace HostedServices.AcceptanceTests.Tests.Api;

[Collection(HostedServicesAcceptanceCollection.Name)]
public sealed partial class EventControllerTests(HostedServicesAcceptanceFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Post_GivenFolderDeleteEvent_ShouldRemoveDescendantFoldersFilesAndContents()
    {
        // Given
        int appId = await CreateAppAsync();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        try
        {
            await SeedFolderDeleteScenarioAsync(appId: appId, roleId: roleId, rootFolderId: rootFolderId, childFolderId: childFolderId, fileId: fileId);

            // When
            HttpStatusCode statusCode = await PostEventAsync(
eventName: "folder_delete", data: new Folder
{
    Id = rootFolderId,
    AppId = appId,
    Name = "content",
    Path = "content",
});

            // Then
            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            await WaitForAsync(
condition: () =>
                {
                    using IServiceScope waitScope = fixture.Factory.Services.CreateScope();

                    using var waitCore = waitScope.ServiceProvider
                        .GetRequiredService<ICoreContextFactory>()
                        .CreateCoreContext();

                    return !waitCore.Set<Folder>()
                        .IgnoreQueryFilters()
                        .Any(predicate: folder => folder.Id == childFolderId);
                }, because: "folder_delete should remove descendant folders");

            using IServiceScope scope = fixture.Factory.Services.CreateScope();

            using var core = scope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Any(predicate: folder => folder.Id == rootFolderId)
                .Should()
                .BeTrue();

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Any(predicate: folder => folder.Id == childFolderId)
                .Should()
                .BeFalse();

            core.Set<DmsFile>()
                .IgnoreQueryFilters()
                .Any(predicate: file => file.Id == fileId)
                .Should()
                .BeFalse();

            core.Set<FileContent>()
                .IgnoreQueryFilters()
                .Any(predicate: content => content.FileId == fileId)
                .Should()
                .BeFalse();

            core.Set<FolderRole>()
                .IgnoreQueryFilters()
                .Any(predicate: folderRole => folderRole.FolderId == childFolderId)
                .Should()
                .BeFalse();
        }
        finally
        {
            await DeleteAppGraphAsync(appId: appId);
        }
    }

    [Fact]
    public async Task Post_GivenAppAddEvent_ShouldCreateSuppliedChildrenAcrossDomains()
    {
        // Given
        int appId = await CreateAppAsync();
        string flowName = Unique(prefix: "Acceptance Flow");

        try
        {
            using IServiceScope seedScope = fixture.Factory.Services.CreateScope();

            using var seedCore = seedScope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            await EnsureCultureAsync(core: seedCore, cultureId: "en-GB", name: "English (UK)");

            // When
            HttpStatusCode statusCode = await PostEventAsync(
eventName: "app_add", data: new AppEntity
{
    Id = appId,
    Cultures =
                    [
                        new AppCulture
                        {
                            AppId = appId,
                            CultureId = "en-GB"
                        }
                    ],
    Folders =
                    [
                        new Folder
                        {
                            AppId = appId,
                            Name = "content"
                        }
                    ],
    MailServers =
                    [
                        new MailServer
                        {
                            AppId = appId,
                            Name = "Acceptance SMTP",
                            User = "user",
                            Password = "pass",
                            Host = "smtp.example.com",
                            FromEmail = "noreply@example.com",
                            Port = 25,
                            EnableSSL = false
                        }
                    ],
    Calendars =
                    [
                        new Calendar
                        {
                            AppId = appId,
                            Name = "Acceptance Calendar",
                            Description = "Acceptance calendar"
                        }
                    ],
    Flows =
                    [
                        new FlowDefinition
                        {
                            Id = Guid.Empty,
                            AppId = appId,
                            Name = flowName,
                            Description = "Acceptance flow",
                            DefinitionJson = "{}",
                            ConfigJson = "{}",
                            CreatedBy = "Guest",
                            CreatedOn = DateTimeOffset.UtcNow,
                            LastUpdatedBy = "Guest",
                            LastUpdated = DateTimeOffset.UtcNow,
                        }
                    ]
});

            // Then
            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            await WaitForAsync(
condition: () =>
                {
                    using IServiceScope waitScope = fixture.Factory.Services.CreateScope();

                    using var waitCore = waitScope.ServiceProvider
                        .GetRequiredService<ICoreContextFactory>()
                        .CreateCoreContext();

                    return waitCore.Set<Role>()
                        .IgnoreQueryFilters()
                        .Any(predicate: role => role.AppId == appId && role.Name == "Administrators")
                        && waitCore.Set<Role>()
                        .IgnoreQueryFilters()
                            .Any(predicate: role => role.AppId == appId && role.Name == "Users")
                        && waitCore.Set<Role>()
                        .IgnoreQueryFilters()
                            .Any(predicate: role => role.AppId == appId && role.Name == "Guests")
                        && waitCore.Set<AppCulture>()
                        .IgnoreQueryFilters()
                            .Any(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")
                        && waitCore.Set<Folder>()
                        .IgnoreQueryFilters()
                            .Any(predicate: folder => folder.AppId == appId && folder.Path == "content")
                        && waitCore.Set<MailServer>()
                        .IgnoreQueryFilters()
                            .Any(predicate: server => server.AppId == appId && server.Name == "Acceptance SMTP")
                        && waitCore.Set<Calendar>()
                        .IgnoreQueryFilters()
                            .Any(predicate: calendar => calendar.AppId == appId && calendar.Name == "Acceptance Calendar")
                        && waitCore.Set<FlowDefinition>()
                        .IgnoreQueryFilters()
                            .Any(predicate: flow => flow.AppId == appId && flow.Name == flowName);
                }, because: "app_add should create cross-domain children");

            using IServiceScope scope = fixture.Factory.Services.CreateScope();

            using var core = scope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            core.Set<Role>()
                .IgnoreQueryFilters()
                .Any(predicate: role => role.AppId == appId && role.Name == "Administrators")
                .Should()
                .BeTrue();

            core.Set<Role>()
                .IgnoreQueryFilters()
                .Any(predicate: role => role.AppId == appId && role.Name == "Users")
                .Should()
                .BeTrue();

            core.Set<Role>()
                .IgnoreQueryFilters()
                .Any(predicate: role => role.AppId == appId && role.Name == "Guests")
                .Should()
                .BeTrue();

            core.Set<AppCulture>()
                .IgnoreQueryFilters()
                .Any(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")
                .Should()
                .BeTrue();

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Any(predicate: folder => folder.AppId == appId && folder.Path == "content")
                .Should()
                .BeTrue();

            core.Set<MailServer>()
                .IgnoreQueryFilters()
                .Any(predicate: server => server.AppId == appId && server.Name == "Acceptance SMTP")
                .Should()
                .BeTrue();

            core.Set<Calendar>()
                .IgnoreQueryFilters()
                .Any(predicate: calendar => calendar.AppId == appId && calendar.Name == "Acceptance Calendar")
                .Should()
                .BeTrue();

            core.Set<FlowDefinition>()
                .IgnoreQueryFilters()
                .Any(predicate: flow => flow.AppId == appId && flow.Name == flowName)
                .Should()
                .BeTrue();
        }
        finally
        {
            await DeleteAppGraphAsync(appId: appId);
        }
    }

    [Fact]
    public async Task Post_GivenAppUpdateEvent_ShouldUpdateChildrenAndRecomputeNestedPaths()
    {
        // Given
        int appId = await CreateAppAsync();
        Guid roleId = Guid.NewGuid();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();

        try
        {
            await SeedAppUpdateScenarioAsync(appId: appId, roleId: roleId, rootFolderId: rootFolderId, childFolderId: childFolderId, fileId: fileId);

            // When
            HttpStatusCode statusCode = await PostEventAsync(
eventName: "app_update", data: new AppEntity
{
    Id = appId,
    Roles =
                    [
                        new Role
                        {
                            Id = roleId,
                            AppId = appId,
                            Name = "Editors",
                            Description = "Updated role",
                            Privs = "app_read,folder_update"
                        }
                    ],
    Cultures =
                    [
                        new AppCulture
                        {
                            AppId = appId,
                            CultureId = "fr-FR"
                        }
                    ],
    Folders =
                    [
                        new Folder
                        {
                            Id = rootFolderId,
                            AppId = appId,
                            Name = "renamed",
                            Path = "renamed"
                        }
                    ]
});

            // Then
            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            await WaitForAsync(
condition: () =>
                {
                    using IServiceScope waitScope = fixture.Factory.Services.CreateScope();

                    using var waitCore = waitScope.ServiceProvider
                        .GetRequiredService<ICoreContextFactory>()
                        .CreateCoreContext();

                    bool roleUpdated = waitCore.Set<Role>()
                        .IgnoreQueryFilters()
                        .Any(predicate: role =>
                            role.Id == roleId
                            && role.Privs == "app_read,folder_update");

                    bool culturesUpdated =
                        waitCore.Set<AppCulture>()
                            .IgnoreQueryFilters()
                            .Any(predicate: culture =>
                                culture.AppId == appId
                                && culture.CultureId == "fr-FR")
                        && !waitCore.Set<AppCulture>()
                            .IgnoreQueryFilters()
                            .Any(predicate: culture =>
                                culture.AppId == appId
                                && culture.CultureId == "en-GB");

                    bool pathsUpdated =
                        waitCore.Set<Folder>()
                            .IgnoreQueryFilters()
                            .Any(predicate: folder =>
                                folder.Id == rootFolderId
                                && folder.Path == "renamed")
                        && waitCore.Set<Folder>()
                            .IgnoreQueryFilters()
                            .Any(predicate: folder =>
                                folder.Id == childFolderId
                                && folder.Path == "renamed/child")
                        && waitCore.Set<DmsFile>()
                            .IgnoreQueryFilters()
                            .Any(predicate: file =>
                                file.Id == fileId
                                && file.Path == "renamed/child/file.txt");

                    return roleUpdated && culturesUpdated && pathsUpdated;
                }, because: "app_update should update children");

            using IServiceScope scope = fixture.Factory.Services.CreateScope();

            using var core = scope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            core.Set<Role>()
                .IgnoreQueryFilters()
                .Single(predicate: role => role.Id == roleId)
                .Privs.Should()
                .Be(expected: "app_read,folder_update");

            core.Set<AppCulture>()
                .IgnoreQueryFilters()
                .Any(predicate: culture => culture.AppId == appId && culture.CultureId == "fr-FR")
                .Should()
                .BeTrue();

            core.Set<AppCulture>()
                .IgnoreQueryFilters()
                .Any(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")
                .Should()
                .BeFalse();

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Single(predicate: folder => folder.Id == rootFolderId)
                .Path.Should()
                .Be(expected: "renamed");

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Single(predicate: folder => folder.Id == childFolderId)
                .Path.Should()
                .Be(expected: "renamed/child");

            core.Set<DmsFile>()
                .IgnoreQueryFilters()
                .Single(predicate: file => file.Id == fileId)
                .Path.Should()
                .Be(expected: "renamed/child/file.txt");
        }
        finally
        {
            await DeleteAppGraphAsync(appId: appId);
        }
    }

    [Fact]
    public async Task Post_GivenAppDeleteEvent_ShouldRemoveCrossDomainChildrenButKeepRootApp()
    {
        // Given
        int appId = await CreateAppAsync();
        Guid roleId = Guid.NewGuid();
        Guid flowId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();

        try
        {
            await SeedAppDeleteScenarioAsync(appId: appId, roleId: roleId, flowId: flowId, folderId: folderId, fileId: fileId);

            // When
            HttpStatusCode statusCode = await PostEventAsync(
eventName: "app_delete", data: new AppEntity
{
    Id = appId
});

            // Then
            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            await WaitForAsync(
condition: () =>
                {
                    using IServiceScope waitScope = fixture.Factory.Services.CreateScope();

                    using var waitCore = waitScope.ServiceProvider
                        .GetRequiredService<ICoreContextFactory>()
                        .CreateCoreContext();

                    return !waitCore.Set<Role>()
                        .IgnoreQueryFilters()
                        .Any(predicate: role => role.AppId == appId);
                }, because: "app_delete should remove cross-domain children");

            using IServiceScope scope = fixture.Factory.Services.CreateScope();

            using var core = scope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            core.Set<AppEntity>()
                .IgnoreQueryFilters()
                .Any(predicate: app => app.Id == appId)
                .Should()
                .BeTrue();

            core.Set<Role>()
                .IgnoreQueryFilters()
                .Any(predicate: role => role.AppId == appId)
                .Should()
                .BeFalse();

            core.Set<UserRole>()
                .IgnoreQueryFilters()
                .Any(predicate: userRole => userRole.RoleId == roleId)
                .Should()
                .BeFalse();

            core.Set<AppCulture>()
                .IgnoreQueryFilters()
                .Any(predicate: culture => culture.AppId == appId)
                .Should()
                .BeFalse();

            core.Set<Folder>()
                .IgnoreQueryFilters()
                .Any(predicate: folder => folder.AppId == appId)
                .Should()
                .BeFalse();

            core.Set<DmsFile>()
                .IgnoreQueryFilters()
                .Any(predicate: file => file.Id == fileId)
                .Should()
                .BeFalse();

            core.Set<FileContent>()
                .IgnoreQueryFilters()
                .Any(predicate: content => content.FileId == fileId)
                .Should()
                .BeFalse();

            core.Set<MailServer>()
                .IgnoreQueryFilters()
                .Any(predicate: server => server.AppId == appId)
                .Should()
                .BeFalse();

            core.Set<Calendar>()
                .IgnoreQueryFilters()
                .Any(predicate: calendar => calendar.AppId == appId)
                .Should()
                .BeFalse();

            core.Set<FlowDefinition>()
                .IgnoreQueryFilters()
                .Any(predicate: flow => flow.Id == flowId)
                .Should()
                .BeFalse();
        }
        finally
        {
            await DeleteAppGraphAsync(appId: appId);
        }
    }

    [Fact]
    public async Task Post_GivenFolderDeleteEvent_ShouldCreateWorkflowInstanceAndTriggerExecutionAttempt()
    {
        // Given
        int appId = await CreateAppAsync();
        Guid roleId = Guid.NewGuid();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        Guid flowId;

        try
        {
            await SeedFolderDeleteScenarioAsync(appId: appId, roleId: roleId, rootFolderId: rootFolderId, childFolderId: childFolderId, fileId: fileId);

            using IServiceScope seedScope = fixture.Factory.Services.CreateScope();

            using var seedCore = seedScope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            flowId = (await seedCore.AddAppFlowDefinitionAsync(flowDefinition: new FlowDefinition
            {
                AppId = appId,
                Name = Unique(prefix: "Subscribed Flow"),
                Description = "Acceptance flow",
                DefinitionJson =
                    "{\"Name\":\"Acceptance\",\"Activities\":[{\"$type\":\"cCoder.Workflow.Activities.Start, cCoder.Workflow.Activities\",\"Ref\":\"start\"}],\"Links\":[]}",
                ConfigJson = "{}",
                CreatedBy = "Guest",
                CreatedOn = DateTimeOffset.UtcNow,
                LastUpdatedBy = "Guest",
                LastUpdated = DateTimeOffset.UtcNow,
            })).Id;

            _ = await seedCore.AddWorkflowEventAsync(workflowEvent: new WorkflowEvent
            {
                FlowId = flowId,
                Type = "Acceptance",
                EventContext = "folder_deletecontent",
                ExecuteAs = "Guest",
                CreatedBy = "Guest",
                CreatedOn = DateTimeOffset.UtcNow,
            });

            // When
            HttpStatusCode statusCode = await PostEventAsync(
eventName: "folder_delete", data: new Folder
{
    Id = rootFolderId,
    AppId = appId,
    Name = "content",
    Path = "content",
});

            // Then
            using IServiceScope scope = fixture.Factory.Services.CreateScope();

            using var core = scope.ServiceProvider
                .GetRequiredService<ICoreContextFactory>()
                .CreateCoreContext();

            statusCode.Should()
                .Be(expected: HttpStatusCode.Accepted);

            await WaitForAsync(
condition: () =>
                {
                    using IServiceScope waitScope = fixture.Factory.Services.CreateScope();

                    using var waitCore = waitScope.ServiceProvider
                        .GetRequiredService<ICoreContextFactory>()
                        .CreateCoreContext();

                    return waitCore.Set<FlowInstanceData>()
                        .IgnoreQueryFilters()
                        .Any(predicate: instance =>
                            instance.FlowDefinitionId == flowId
                            && instance.State != "Queued");
                }, because: "folder_delete should create and execute the subscribed workflow instance");

            FlowInstanceData instance = core.Set<FlowInstanceData>()
                .IgnoreQueryFilters()
                .Single(predicate: instance => instance.FlowDefinitionId == flowId);

            instance.State.Should()
                .NotBe(unexpected: "Queued");
        }
        finally
        {
            await DeleteAppGraphAsync(appId: appId);
        }
    }

    private async Task<int> CreateAppAsync()
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        AppEntity app = await core.AddAppAsync(app: new AppEntity
        {
            Name = Unique(prefix: "AcceptanceApp"),
            Domain = $"{Unique(prefix: "acceptance")}.local",
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            TenantId = Unique(prefix: "tenant"),
            ConfigJson = "{}",
        });

        Role appAdministratorRole = await core.AddRoleAsync(role: new Role
        {
            Id = Guid.NewGuid(),
            AppId = app.Id,
            Name = Unique(prefix: "Acceptance Administrators"),
            Description = "Acceptance event administrator",
            Privs = AcceptanceApplicationSeeder.AcceptanceAdminPrivileges
        });

        await core.AddUserRoleAsync(userRole: new UserRole
        {
            RoleId = appAdministratorRole.Id,
            UserId = "Guest"
        });

        return app.Id;
    }

    private async Task<HttpStatusCode> PostEventAsync(
        string eventName,
        object data,
        string ssoUserId = "Guest")
    {
        using HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
requestUri: "/Api/Eventing", value: new HttpEventMessage
{
    EventName = eventName,
    SSOUserId = ssoUserId,
    Data = JsonSerializer.Serialize(value: data, options: JsonOptions),
});

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Accepted, because: content);

        return response.StatusCode;
    }

    private static async Task WaitForAsync(
        Func<bool> condition,
        string because)
    {
        DateTimeOffset stopAt = DateTimeOffset.UtcNow.AddSeconds(seconds: 75);
        Exception lastException = null;

        while (DateTimeOffset.UtcNow < stopAt)
        {
            try
            {
                if (condition())
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                lastException = exception;
            }

            await Task.Delay(millisecondsDelay: 100);
        }

        if (lastException is not null)
        {
            throw new TimeoutException($"Timed out waiting because {because}.", lastException);
        }

        throw new TimeoutException($"Timed out waiting because {because}.");
    }

    private async Task SeedFolderDeleteScenarioAsync(
        int appId,
        Guid roleId,
        Guid rootFolderId,
        Guid childFolderId,
        Guid fileId)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        await core.AddRoleAsync(role: new Role
        {
            Id = roleId,
            AppId = appId,
            Name = Unique(prefix: "FolderDeleteRole"),
            Description = "Acceptance role",
            Privs = "app_admin,folder_delete,file_delete,flowdefinition_execute"
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = roleId, UserId = "Guest" });

        await core.AddFolderAsync(folder: new Folder
        {
            Id = rootFolderId,
            AppId = appId,
            Name = "content",
            Path = "content"
        });

        await core.AddFolderAsync(folder: new Folder
        {
            Id = childFolderId,
            AppId = appId,
            ParentId = rootFolderId,
            Name = "child",
            Path = "content/child"
        });

        await core.AddFolderRoleAsync(folderRole: new FolderRole { FolderId = rootFolderId, RoleId = roleId });
        await core.AddFolderRoleAsync(folderRole: new FolderRole { FolderId = childFolderId, RoleId = roleId });

        await core.AddDmsFileAsync(file: new DmsFile
        {
            Id = fileId,
            FolderId = childFolderId,
            Name = "file.txt",
            Path = "content/child/file.txt",
            MimeType = "text/plain",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            Size = "1 B"
        });

        await core.AddFileContentAsync(fileContent: new FileContent
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            Description = "content",
            Size = "1 B",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            Version = 1,
            RawData = [1]
        });
    }

    private async Task SeedAppUpdateScenarioAsync(
        int appId,
        Guid roleId,
        Guid rootFolderId,
        Guid childFolderId,
        Guid fileId)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        await EnsureCultureAsync(core: core, cultureId: "en-GB", name: "English (UK)");
        await EnsureCultureAsync(core: core, cultureId: "fr-FR", name: "French");

        await core.AddRoleAsync(role: new Role
        {
            Id = roleId,
            AppId = appId,
            Name = "Editors",
            Description = "Original role",
            Privs = "app_admin,app_read,folder_update"
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = roleId, UserId = "Guest" });

        await core.AddAppCultureAsync(appCulture: new AppCulture
        {
            AppId = appId,
            CultureId = "en-GB"
        });

        await core.AddFolderAsync(folder: new Folder
        {
            Id = rootFolderId,
            AppId = appId,
            Name = "content",
            Path = "content"
        });

        await core.AddFolderAsync(folder: new Folder
        {
            Id = childFolderId,
            AppId = appId,
            ParentId = rootFolderId,
            Name = "child",
            Path = "content/child"
        });

        await core.AddDmsFileAsync(file: new DmsFile
        {
            Id = fileId,
            FolderId = childFolderId,
            Name = "file.txt",
            Path = "content/child/file.txt",
            MimeType = "text/plain",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            Size = "1 B"
        });
    }

    private async Task SeedAppDeleteScenarioAsync(
        int appId,
        Guid roleId,
        Guid flowId,
        Guid folderId,
        Guid fileId)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        await EnsureCultureAsync(core: core, cultureId: "en-GB", name: "English (UK)");

        await core.AddRoleAsync(role: new Role
        {
            Id = roleId,
            AppId = appId,
            Name = Unique(prefix: "DeleteRole"),
            Description = "Delete role",
            Privs = "app_admin,app_delete,folder_delete,file_delete"
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = roleId, UserId = "Guest" });

        await core.AddAppCultureAsync(appCulture: new AppCulture
        {
            AppId = appId,
            CultureId = "en-GB"
        });

        await core.AddFolderAsync(folder: new Folder
        {
            Id = folderId,
            AppId = appId,
            Name = "content",
            Path = "content"
        });

        await core.AddFolderRoleAsync(folderRole: new FolderRole
        {
            FolderId = folderId,
            RoleId = roleId
        });

        await core.AddDmsFileAsync(file: new DmsFile
        {
            Id = fileId,
            FolderId = folderId,
            Name = "file.txt",
            Path = "content/file.txt",
            MimeType = "text/plain",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            Size = "1 B"
        });

        await core.AddFileContentAsync(fileContent: new FileContent
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            Description = "content",
            Size = "1 B",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            Version = 1,
            RawData = [1]
        });

        await core.AddMailServerAsync(mailServer: new MailServer
        {
            AppId = appId,
            Name = "Delete SMTP",
            User = "user",
            Password = "pass",
            Host = "smtp.example.com",
            FromEmail = "noreply@example.com",
            Port = 25,
            EnableSSL = false
        });

        await core.AddCalendarAsync(calendar: new Calendar
        {
            AppId = appId,
            Name = "Delete Calendar",
            Description = "Calendar"
        });

        await core.AddAppFlowDefinitionAsync(flowDefinition: new FlowDefinition
        {
            Id = flowId,
            AppId = appId,
            Name = "Delete Flow",
            Description = "Flow",
            DefinitionJson = "{}",
            ConfigJson = "{}",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            LastUpdatedBy = "Guest",
            LastUpdated = DateTimeOffset.UtcNow,
        });
    }

    private async Task DeleteAppGraphAsync(int appId)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        Guid[] roleIds =
            [.. core.Set<Role>()
            .IgnoreQueryFilters()
                .Where(predicate: role => role.AppId == appId)
                .Select(selector: role => role.Id)];

        await core.DeleteAllAsync(
userRoles: [.. core.Set<UserRole>()
            .IgnoreQueryFilters()
                .Where(predicate: userRole => roleIds.Contains(value: userRole.RoleId))]);

        await core.DeleteAllAsync(
folderRoles: [.. core.Set<FolderRole>()
            .IgnoreQueryFilters()
                .Where(predicate: folderRole => roleIds.Contains(value: folderRole.RoleId))]);

        Guid[] folderIds =
            [.. core.Set<Folder>()
            .IgnoreQueryFilters()
                .Where(predicate: folder => folder.AppId == appId)
                .Select(selector: folder => folder.Id)];

        Guid[] fileIds =
            [.. core.Set<DmsFile>()
            .IgnoreQueryFilters()
                .Where(predicate: file => folderIds.Contains(value: file.FolderId))
                .Select(selector: file => file.Id)];

        await core.DeleteAllAsync(
fileContents: [.. core.Set<FileContent>()
            .IgnoreQueryFilters()
                .Where(predicate: content => fileIds.Contains(value: content.FileId))]);

        await core.DeleteAllAsync(
files: [.. core.Set<DmsFile>()
            .IgnoreQueryFilters()
                .Where(predicate: file => fileIds.Contains(value: file.Id))]);

        await core.DeleteAllAsync(
folders: [.. core.Set<Folder>()
            .IgnoreQueryFilters()
                .Where(predicate: folder => folderIds.Contains(value: folder.Id))
                .OrderByDescending(keySelector: folder => folder.Path.Length)]);

        await core.DeleteAllAsync(
mailServers: [.. core.Set<MailServer>()
            .IgnoreQueryFilters()
                .Where(predicate: server => server.AppId == appId)]);

        await core.DeleteAllAsync(
queuedEmails: [.. core.Set<QueuedEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: email => email.AppId == appId)]);

        await core.DeleteAllAsync(
sentEmails: [.. core.Set<SentEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: email => email.AppId == appId)]);

        await core.DeleteAllAsync(
calendars: [.. core.Set<Calendar>()
            .IgnoreQueryFilters()
                .Where(predicate: calendar => calendar.AppId == appId)]);

        await core.DeleteAllAsync(
scheduledTasks: [.. core.Set<ScheduledTask>()
            .IgnoreQueryFilters()
                .Where(predicate: task => task.AppId == appId)]);

        Guid[] flowIds =
            [.. core.Set<FlowDefinition>()
            .IgnoreQueryFilters()
                .Where(predicate: flow => flow.AppId == appId)
                .Select(selector: flow => flow.Id)];

        await core.DeleteAllAsync(
workflowEvents: [.. core.Set<WorkflowEvent>()
            .IgnoreQueryFilters()
                .Where(predicate: workflowEvent => flowIds.Contains(value: workflowEvent.FlowId))]);

        await core.DeleteAllAsync(
flowInstances: [.. core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
                .Where(predicate: instance => flowIds.Contains(value: instance.FlowDefinitionId))]);

        await core.DeleteAllAsync(
flowDefinitions: [.. core.Set<FlowDefinition>()
            .IgnoreQueryFilters()
                .Where(predicate: flow => flow.AppId == appId)]);

        await core.DeleteAllAsync(
appCultures: [.. core.Set<AppCulture>()
            .IgnoreQueryFilters()
                .Where(predicate: culture => culture.AppId == appId)]);

        await core.DeleteAllAsync(
roles: [.. core.Set<Role>()
            .IgnoreQueryFilters()
                .Where(predicate: role => role.AppId == appId)]);

        AppEntity app = core.Set<AppEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefault(predicate: foundApp => foundApp.Id == appId);

        if (app is not null)
        {
            await core.DeleteAsync(app: app);
        }
    }

    private static async Task EnsureCultureAsync(
        CoreDataContext core,
        string cultureId,
        string name)
    {
        bool exists = await core.Set<Culture>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: culture => culture.Id == cultureId);

        if (!exists)
        {
            await core.AddCultureAsync(culture: new Culture
            {
                Id = cultureId,
                Name = name
            });
        }
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";
}