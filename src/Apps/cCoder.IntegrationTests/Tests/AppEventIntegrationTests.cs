// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using cCoder.IntegrationTests.Infrastructure;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AppEntity = cCoder.Data.Models.CMS.App;
using DmsFile = cCoder.Data.Models.DMS.File;
using SsoToken = cCoder.Security.Objects.Entities.Token;

namespace cCoder.IntegrationTests.Tests;

[Collection(IntegrationAcceptanceCollection.Name)]
public sealed partial class AppEventIntegrationTests
{
    private const int BaselineAppId = 1;
    private const string AdminUserId = "admin";
    private const string SimpleFlowDefinitionJson =
        "{\"Name\":\"Acceptance\",\"Activities\":[{\"$type\":\"cCoder.Workflow.Activities.Start, cCoder.Workflow.Activities\",\"Ref\":\"start\"}],\"Links\":[]}";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions RequestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IntegrationAcceptanceFixture fixture;

    public AppEventIntegrationTests(IntegrationAcceptanceFixture fixture) =>
        this.fixture = fixture;

    private async Task<int> CreateStandaloneAppAsync(string domain)
    {
        await using CoreDataContext core = CreateCoreContext();

        AppEntity app = await core.AddAppAsync(app: new AppEntity
        {
            Name = Unique(prefix: "IntegrationApp"),
            Domain = domain,
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            TenantId = Unique(prefix: "tenant"),
            ConfigJson = "{}"
        });

        return app.Id;
    }

    private async Task GrantGuestAdminAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        Role templateRole = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: role => role.AppId == BaselineAppId && role.Name == "Acceptance Administrators");

        Role role = await core.Set<Role>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(predicate: found => found.AppId == appId && found.Name == templateRole.Name);

        if (role is null)
        {
            role = await core.AddRoleAsync(role: new Role
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                Name = templateRole.Name,
                Description = templateRole.Description,
                Privs = templateRole.Privs
            });
        }

        bool hasGuestRole = await core.Set<UserRole>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: userRole => userRole.RoleId == role.Id && userRole.UserId == "Guest");

        if (!hasGuestRole)
        {
            await core.AddUserRoleAsync(userRole: new UserRole { RoleId = role.Id, UserId = "Guest" });
        }
    }

    private async Task SeedAppUpdateScenarioAsync(
        int appId,
        Guid roleId,
        Guid rootFolderId,
        Guid childFolderId,
        Guid fileId)
    {
        await using CoreDataContext core = CreateCoreContext();

        await EnsureCultureAsync(cultureId: "en-GB",name: "English (UK)");
        await EnsureCultureAsync(cultureId: "fr-FR",name: "French");

        await core.AddRoleAsync(role: new Role
        {
            Id = roleId,
            AppId = appId,
            Name = "Editors",
            Description = "Original role",
            Privs = "app_admin,app_read,folder_update"
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = roleId, UserId = "Guest" });
        await core.AddAppCultureAsync(appCulture: new AppCulture { AppId = appId, CultureId = "en-GB" });
        await core.AddFolderAsync(folder: new Folder { Id = rootFolderId, AppId = appId, Name = "content", Path = "content" });
        await core.AddFolderAsync(folder: new Folder { Id = childFolderId, AppId = appId, ParentId = rootFolderId, Name = "child", Path = "content/child" });

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
        await using CoreDataContext core = CreateCoreContext();

        await EnsureCultureAsync(cultureId: "en-GB",name: "English (UK)");

        await core.AddRoleAsync(role: new Role
        {
            Id = roleId,
            AppId = appId,
            Name = Unique(prefix: "DeleteRole"),
            Description = "Delete role",
            Privs = "app_admin,app_delete,AppCulture_delete,folder_delete,file_delete"
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = roleId, UserId = "Guest" });
        await core.AddAppCultureAsync(appCulture: new AppCulture { AppId = appId, CultureId = "en-GB" });
        await core.AddFolderAsync(folder: new Folder { Id = folderId, AppId = appId, Name = "content", Path = "content" });
        await core.AddFolderRoleAsync(folderRole: new FolderRole { FolderId = folderId, RoleId = roleId });

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

        await core.AddCalendarAsync(calendar: new Calendar { AppId = appId, Name = "Delete Calendar", Description = "Calendar" });

        await core.AddAppFlowDefinitionAsync(flowDefinition: new FlowDefinition
        {
            Id = flowId,
            AppId = appId,
            Name = "Delete Flow",
            Description = "Flow",
            DefinitionJson = SimpleFlowDefinitionJson,
            ConfigJson = "{}",
            CreatedBy = "Guest",
            CreatedOn = DateTimeOffset.UtcNow,
            LastUpdatedBy = "Guest",
            LastUpdated = DateTimeOffset.UtcNow
        });
    }

    private async Task DeleteAppGraphAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        Guid[] roleIds =
            [.. await core.Set<Role>()
            .IgnoreQueryFilters()
                .Where(predicate: role => role.AppId == appId)
                .Select(selector: role => role.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
userRoles:             core.Set<UserRole>()
            .IgnoreQueryFilters()
                .Where(predicate: userRole => roleIds.Contains(value: userRole.RoleId))
                .ToArray());

        await core.DeleteAllAsync(
folderRoles:             core.Set<FolderRole>()
            .IgnoreQueryFilters()
                .Where(predicate: folderRole => roleIds.Contains(value: folderRole.RoleId))
                .ToArray());

        Guid[] folderIds =
            [.. await core.Set<Folder>()
            .IgnoreQueryFilters()
                .Where(predicate: folder => folder.AppId == appId)
                .Select(selector: folder => folder.Id)
                .ToArrayAsync()];

        Guid[] fileIds =
            [.. await core.Set<DmsFile>()
            .IgnoreQueryFilters()
                .Where(predicate: file => folderIds.Contains(value: file.FolderId))
                .Select(selector: file => file.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
fileContents:             core.Set<FileContent>()
            .IgnoreQueryFilters()
                .Where(predicate: content => fileIds.Contains(value: content.FileId))
                .ToArray());

        await core.DeleteAllAsync(
files:             core.Set<DmsFile>()
            .IgnoreQueryFilters()
                .Where(predicate: file => fileIds.Contains(value: file.Id))
                .ToArray());

        await core.DeleteAllAsync(
folders:             core.Set<Folder>()
            .IgnoreQueryFilters()
                .Where(predicate: folder => folderIds.Contains(value: folder.Id))
                .OrderByDescending(keySelector: folder => folder.Path.Length)
                .ToArray());

        await core.DeleteAllAsync(
mailServers:             core.Set<MailServer>()
            .IgnoreQueryFilters()
                .Where(predicate: server => server.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
calendars:             core.Set<Calendar>()
            .IgnoreQueryFilters()
                .Where(predicate: calendar => calendar.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
queuedEmails:             core.Set<QueuedEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: email => email.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
sentEmails:             core.Set<SentEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: email => email.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
scheduledTasks:             core.Set<ScheduledTask>()
            .IgnoreQueryFilters()
                .Where(predicate: task => task.AppId == appId)
                .ToArray());

        Guid[] flowIds =
            [.. await core.Set<FlowDefinition>()
            .IgnoreQueryFilters()
                .Where(predicate: flow => flow.AppId == appId)
                .Select(selector: flow => flow.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
workflowEvents:             core.Set<WorkflowEvent>()
            .IgnoreQueryFilters()
                .Where(predicate: workflowEvent => flowIds.Contains(value: workflowEvent.FlowId))
                .ToArray());

        await core.DeleteAllAsync(
flowInstances:             core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
                .Where(predicate: instance => flowIds.Contains(value: instance.FlowDefinitionId))
                .ToArray());

        await core.DeleteAllAsync(
flowDefinitions:             core.Set<FlowDefinition>()
            .IgnoreQueryFilters()
                .Where(predicate: flow => flow.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
appCultures:             core.Set<AppCulture>()
            .IgnoreQueryFilters()
                .Where(predicate: culture => culture.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
roles:             core.Set<Role>()
            .IgnoreQueryFilters()
                .Where(predicate: role => role.AppId == appId)
                .ToArray());

        AppEntity app = await core.Set<AppEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(predicate: found => found.Id == appId);

        if (app is not null)
        {
            await core.DeleteAsync(app: app);
        }
    }

    private async Task EnsureCultureAsync(string cultureId, string name)
    {
        await using CoreDataContext core = CreateCoreContext();

        bool exists = await core.Set<Culture>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: culture => culture.Id == cultureId);

        if (!exists)
        {
            await core.AddCultureAsync(culture: new Culture { Id = cultureId, Name = name });
        }
    }

    private async Task<object> PostAsJsonAsync(
        string relativeUrl,
        object payload,
        Type responseType,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(inputValue: payload,options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(value: authToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authToken);
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return JsonSerializer.Deserialize(json: content, returnType: responseType, options: JsonOptions)
            ?? throw new InvalidOperationException($"Expected payload for {relativeUrl}.");
    }

    private async Task SendAsJsonAsync(HttpMethod method, string relativeUrl, object payload, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl)
        {
            Content = JsonContent.Create(inputValue: payload,options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(value: host))
        {
            request.Headers.Host = host;
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);
    }

    private async Task SendWithOptionalHostAsync(HttpMethod method, string relativeUrl, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl);

        if (!string.IsNullOrWhiteSpace(value: host))
        {
            request.Headers.Host = host;
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: BuildFailureMessage(content: content));
    }

    private string BuildFailureMessage(string content) =>
        $"""
        {content}

        Web output:
        {Tail(value: fixture.WebOutput)}

        HostedServices output:
        {Tail(value: fixture.HostedServicesOutput)}
        """;

    private async Task<string> BuildEventDiagnosticsAsync(
        int appId,
        Guid rootFolderId,
        Guid childFolderId,
        Guid fileId)
    {
        await using CoreDataContext core = CreateCoreContext();

        string[] folders = await core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(predicate: folder =>
                folder.AppId == appId
                || folder.Id == rootFolderId
                || folder.Id == childFolderId)
            .Select(selector: folder =>
                $"{folder.Id}: Parent={folder.ParentId}, Name={folder.Name}, Path={folder.Path}")
            .ToArrayAsync();

        string[] files = await core.Set<DmsFile>()
            .IgnoreQueryFilters()
            .Where(predicate: file => file.Id == fileId)
            .Select(selector: file =>
                $"{file.Id}: Folder={file.FolderId}, Name={file.Name}, Path={file.Path}")
            .ToArrayAsync();

        return $"""
            App event state:
            Folders:
            {string.Join(separator: Environment.NewLine, values: folders)}
            Files:
            {string.Join(separator: Environment.NewLine, values: files)}

            Web output:
            {Tail(value: fixture.WebOutput)}

            HostedServices output:
            {Tail(value: fixture.HostedServicesOutput)}
            """;
    }

    private static string Tail(string value, int length = 30000)
    {
        if (string.IsNullOrWhiteSpace(value: value) || value.Length <= length)
        {
            return value ?? string.Empty;
        }

        return value[^length..];
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        int attempts = 60,
        int delayMilliseconds = 500,
        Func<Task<string>> diagnosticsFactory = null)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (await predicate())
            {
                return;
            }

            await Task.Delay(millisecondsDelay: delayMilliseconds);
        }

        string diagnostics = diagnosticsFactory is null
            ? string.Empty
            : await diagnosticsFactory();

        throw new TimeoutException(
            $"Timed out waiting for the expected condition.{Environment.NewLine}{diagnostics}");
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        string tokenId = Guid.NewGuid()
            .ToString(format: "N");

        sso.Add(entity: new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(hours: 1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";
}