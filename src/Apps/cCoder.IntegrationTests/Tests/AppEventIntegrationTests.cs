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
        AppEntity app = await core.AddAppAsync(new AppEntity
        {
            Name = Unique("IntegrationApp"),
            Domain = domain,
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            TenantId = Unique("tenant"),
            ConfigJson = "{}"
        });

        return app.Id;
    }

    private async Task GrantGuestAdminAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        Role templateRole = await core.Set<Role>().IgnoreQueryFilters()
            .SingleAsync(role => role.AppId == BaselineAppId && role.Name == "Acceptance Administrators");

        Role role = await core.Set<Role>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(found => found.AppId == appId && found.Name == templateRole.Name);

        if (role is null)
        {
            role = await core.AddRoleAsync(new Role
            {
                Id = Guid.NewGuid(),
                AppId = appId,
                Name = templateRole.Name,
                Description = templateRole.Description,
                Privs = templateRole.Privs
            });
        }

        bool hasGuestRole = await core.Set<UserRole>().IgnoreQueryFilters()
            .AnyAsync(userRole => userRole.RoleId == role.Id && userRole.UserId == "Guest");

        if (!hasGuestRole)
            await core.AddUserRoleAsync(new UserRole { RoleId = role.Id, UserId = "Guest" });
    }

    private async Task SeedAppUpdateScenarioAsync(
        int appId,
        Guid roleId,
        Guid rootFolderId,
        Guid childFolderId,
        Guid fileId)
    {
        await using CoreDataContext core = CreateCoreContext();

        await EnsureCultureAsync("en-GB", "English (UK)");
        await EnsureCultureAsync("fr-FR", "French");

        await core.AddRoleAsync(new Role
        {
            Id = roleId,
            AppId = appId,
            Name = "Editors",
            Description = "Original role",
            Privs = "app_admin,app_read,folder_update"
        });

        await core.AddUserRoleAsync(new UserRole { RoleId = roleId, UserId = "Guest" });
        await core.AddAppCultureAsync(new AppCulture { AppId = appId, CultureId = "en-GB" });
        await core.AddFolderAsync(new Folder { Id = rootFolderId, AppId = appId, Name = "content", Path = "content" });
        await core.AddFolderAsync(new Folder { Id = childFolderId, AppId = appId, ParentId = rootFolderId, Name = "child", Path = "content/child" });
        await core.AddDmsFileAsync(new DmsFile
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

        await EnsureCultureAsync("en-GB", "English (UK)");
        await core.AddRoleAsync(new Role
        {
            Id = roleId,
            AppId = appId,
            Name = Unique("DeleteRole"),
            Description = "Delete role",
            Privs = "app_admin,app_delete,folder_delete,file_delete"
        });

        await core.AddUserRoleAsync(new UserRole { RoleId = roleId, UserId = "Guest" });
        await core.AddAppCultureAsync(new AppCulture { AppId = appId, CultureId = "en-GB" });
        await core.AddFolderAsync(new Folder { Id = folderId, AppId = appId, Name = "content", Path = "content" });
        await core.AddFolderRoleAsync(new FolderRole { FolderId = folderId, RoleId = roleId });
        await core.AddDmsFileAsync(new DmsFile
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
        await core.AddFileContentAsync(new FileContent
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
        await core.AddMailServerAsync(new MailServer
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
        await core.AddCalendarAsync(new Calendar { AppId = appId, Name = "Delete Calendar", Description = "Calendar" });
        await core.AddAppFlowDefinitionAsync(new FlowDefinition
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
            [.. await core.Set<Role>().IgnoreQueryFilters()
                .Where(role => role.AppId == appId)
                .Select(role => role.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
            core.Set<UserRole>().IgnoreQueryFilters()
                .Where(userRole => roleIds.Contains(userRole.RoleId))
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<FolderRole>().IgnoreQueryFilters()
                .Where(folderRole => roleIds.Contains(folderRole.RoleId))
                .ToArray());

        Guid[] folderIds =
            [.. await core.Set<Folder>().IgnoreQueryFilters()
                .Where(folder => folder.AppId == appId)
                .Select(folder => folder.Id)
                .ToArrayAsync()];

        Guid[] fileIds =
            [.. await core.Set<DmsFile>().IgnoreQueryFilters()
                .Where(file => folderIds.Contains(file.FolderId))
                .Select(file => file.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
            core.Set<FileContent>().IgnoreQueryFilters()
                .Where(content => fileIds.Contains(content.FileId))
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<DmsFile>().IgnoreQueryFilters()
                .Where(file => fileIds.Contains(file.Id))
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<Folder>().IgnoreQueryFilters()
                .Where(folder => folderIds.Contains(folder.Id))
                .OrderByDescending(folder => folder.Path.Length)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<MailServer>().IgnoreQueryFilters()
                .Where(server => server.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<Calendar>().IgnoreQueryFilters()
                .Where(calendar => calendar.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<QueuedEmail>().IgnoreQueryFilters()
                .Where(email => email.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<SentEmail>().IgnoreQueryFilters()
                .Where(email => email.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<ScheduledTask>().IgnoreQueryFilters()
                .Where(task => task.AppId == appId)
                .ToArray());

        Guid[] flowIds =
            [.. await core.Set<FlowDefinition>().IgnoreQueryFilters()
                .Where(flow => flow.AppId == appId)
                .Select(flow => flow.Id)
                .ToArrayAsync()];

        await core.DeleteAllAsync(
            core.Set<WorkflowEvent>().IgnoreQueryFilters()
                .Where(workflowEvent => flowIds.Contains(workflowEvent.FlowId))
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<FlowInstanceData>().IgnoreQueryFilters()
                .Where(instance => flowIds.Contains(instance.FlowDefinitionId))
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<FlowDefinition>().IgnoreQueryFilters()
                .Where(flow => flow.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<AppCulture>().IgnoreQueryFilters()
                .Where(culture => culture.AppId == appId)
                .ToArray());

        await core.DeleteAllAsync(
            core.Set<Role>().IgnoreQueryFilters()
                .Where(role => role.AppId == appId)
                .ToArray());

        AppEntity app = await core.Set<AppEntity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(found => found.Id == appId);

        if (app is not null)
            await core.DeleteAsync(app);
    }

    private async Task EnsureCultureAsync(string cultureId, string name)
    {
        await using CoreDataContext core = CreateCoreContext();
        bool exists = await core.Set<Culture>().IgnoreQueryFilters()
            .AnyAsync(culture => culture.Id == cultureId);

        if (!exists)
            await core.AddCultureAsync(new Culture { Id = cultureId, Name = name });
    }

    private async Task<T> PostAsJsonAsync<T>(
        string relativeUrl,
        object payload,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(payload, options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authToken);
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException($"Expected payload for {relativeUrl}.");
    }

    private async Task SendAsJsonAsync(HttpMethod method, string relativeUrl, object payload, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl)
        {
            Content = JsonContent.Create(payload, options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(host))
            request.Headers.Host = host;

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    private async Task SendWithOptionalHostAsync(HttpMethod method, string relativeUrl, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl);

        if (!string.IsNullOrWhiteSpace(host))
            request.Headers.Host = host;

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        int attempts = 60,
        int delayMilliseconds = 500)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (await predicate())
                return;

            await Task.Delay(delayMilliseconds);
        }

        throw new TimeoutException("Timed out waiting for the expected condition.");
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>().CreateCoreContext();

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        string tokenId = Guid.NewGuid().ToString("N");

        sso.Add(new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";
}
