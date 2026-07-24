// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Web.AcceptanceTests.Infrastructure;
using Xunit;
using DmsFile = cCoder.Data.Models.DMS.File;
using AppFlowDefinition = cCoder.Data.Models.Workflow.FlowDefinition;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed partial class AppDeleteCascadeTests(WebAcceptanceFixture fixture)
{
    private string BaseUrl { get; } = "/Api/Core/App";

    private sealed record SeededApp(int AppId, Guid RoleId, string Domain);

    [Fact]
    public async Task Delete_RemovesCrossDomainChildren()
    {
        // Given
        SeededApp seededApp = await SeedDatabase(
            privileges:
            [
                "app_delete",
                "folder_delete",
                "file_delete",
                "mailserver_delete",
                "queuedemail_delete",
                "sentemail_delete",
                "role_delete",
                "scheduledtask_delete",
                "userrole_delete",
                "flowdefinition_delete"
            ]);

        Guid flowId = Guid.NewGuid();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();

        try
        {
            using (IServiceScope scope = fixture.Factory.Services.CreateScope())
            {
                using var core = scope.ServiceProvider
                    .GetRequiredService<cCoder.Data.ICoreContextFactory>()
                    .CreateCoreContext();

                await core.AddAppFlowDefinitionAsync(flowDefinition: new AppFlowDefinition
                {
                    Id = flowId,
                    AppId = seededApp.AppId,
                    Name = "Acceptance Flow",
                    Description = "Acceptance flow",
                    DefinitionJson = "{}",
                    ConfigJson = "{}",
                    CreatedBy = "Guest",
                    CreatedOn = DateTimeOffset.UtcNow,
                    LastUpdatedBy = "Guest",
                    LastUpdated = DateTimeOffset.UtcNow
                });

                await core.AddScheduledTaskAsync(scheduledTask: new ScheduledTask
                {
                    AppId = seededApp.AppId,
                    FlowId = flowId,
                    Name = "Acceptance Task",
                    Description = "Task",
                    ExecutionArgs = "{}",
                    ScheduleInTicks = TimeSpan.FromMinutes(minutes: 5).Ticks,
                    CreatedBy = "Guest",
                    UpdatedBy = "Guest",
                    ExecuteAs = "Guest",
                    Created = DateTimeOffset.UtcNow,
                    LastUpdated = DateTimeOffset.UtcNow
                });

                await core.AddMailServerAsync(mailServer: new MailServer
                {
                    AppId = seededApp.AppId,
                    Name = "Acceptance Server",
                    User = "user",
                    Password = "pass",
                    Host = "smtp.example.com",
                    FromEmail = "noreply@example.com",
                    Port = 25,
                    EnableSSL = false
                });

                await core.AddQueuedEmailAsync(queuedEmail: new QueuedEmail
                {
                    AppId = seededApp.AppId,
                    SentByUserId = "Guest",
                    Subject = "Queued",
                    Content = "Queued content",
                    To = "guest@example.com",
                    CC = "",
                    MailServerName = "Acceptance Server"
                });

                await core.AddSentEmailAsync(sentEmail: new SentEmail
                {
                    AppId = seededApp.AppId,
                    SentByUserId = "Guest",
                    Subject = "Sent",
                    Content = "Sent content",
                    To = "guest@example.com",
                    CC = "",
                    SentOn = DateTimeOffset.UtcNow,
                    From = "noreply@example.com"
                });

                await core.AddFolderAsync(folder: new Folder
                {
                    Id = rootFolderId,
                    AppId = seededApp.AppId,
                    Name = "Root",
                    Path = "root"
                });

                await core.AddFolderAsync(folder: new Folder
                {
                    Id = childFolderId,
                    AppId = seededApp.AppId,
                    ParentId = rootFolderId,
                    Name = "Child",
                    Path = "root/child"
                });

                await core.AddFolderRoleAsync(folderRole: new FolderRole { FolderId = rootFolderId, RoleId = seededApp.RoleId });
                await core.AddFolderRoleAsync(folderRole: new FolderRole { FolderId = childFolderId, RoleId = seededApp.RoleId });

                await core.AddDmsFileAsync(file: new DmsFile
                {
                    Id = fileId,
                    FolderId = childFolderId,
                    Name = "file.txt",
                    Path = "root/child/file.txt",
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

            // When
            int actualStatusCode = await DeleteAppAsync(host: seededApp.Domain,id: seededApp.AppId);

            using IServiceScope verificationScope = fixture.Factory.Services.CreateScope();

            using var verifyCore = verificationScope.ServiceProvider
                .GetRequiredService<cCoder.Data.ICoreContextFactory>()
                .CreateCoreContext();

            // Then
            actualStatusCode.Should()
                .Be(expected: 200);

            verifyCore.Set<App>()
                .IgnoreQueryFilters()
                .Any(predicate: app => app.Id == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<Role>()
                .IgnoreQueryFilters()
                .Any(predicate: role => role.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<UserRole>()
                .IgnoreQueryFilters()
                .Any(predicate: userRole => userRole.RoleId == seededApp.RoleId)
                .Should()
                .BeFalse();

            verifyCore.Set<FolderRole>()
                .IgnoreQueryFilters()
                .Any(predicate: folderRole => folderRole.RoleId == seededApp.RoleId)
                .Should()
                .BeFalse();

            verifyCore.Set<Folder>()
                .IgnoreQueryFilters()
                .Any(predicate: folder => folder.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<DmsFile>()
                .IgnoreQueryFilters()
                .Any(predicate: file => file.FolderId == childFolderId)
                .Should()
                .BeFalse();

            verifyCore.Set<FileContent>()
                .IgnoreQueryFilters()
                .Any(predicate: content => content.FileId == fileId)
                .Should()
                .BeFalse();

            verifyCore.Set<MailServer>()
                .IgnoreQueryFilters()
                .Any(predicate: server => server.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<QueuedEmail>()
                .IgnoreQueryFilters()
                .Any(predicate: email => email.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<SentEmail>()
                .IgnoreQueryFilters()
                .Any(predicate: email => email.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<ScheduledTask>()
                .IgnoreQueryFilters()
                .Any(predicate: task => task.AppId == seededApp.AppId)
                .Should()
                .BeFalse();

            verifyCore.Set<AppFlowDefinition>()
                .IgnoreQueryFilters()
                .Any(predicate: flow => flow.AppId == seededApp.AppId)
                .Should()
                .BeFalse();
        }
        finally
        {
            await Teardown(seededApp: seededApp);
        }
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";

    private async Task<SeededApp> SeedDatabase(params string[] privileges)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<cCoder.Data.ICoreContextFactory>()
            .CreateCoreContext();

        App app = await core.AddAppAsync(app: new App
        {
            Name = Unique(prefix: "AcceptanceApp"),
            Domain = $"{Unique(prefix: "acceptance")}.local",
            DefaultTheme = "Default",
            DefaultCultureId = string.Empty,
            TenantId = Unique(prefix: "tenant"),
            ConfigJson = "{}",
        });

        Role role = await core.AddRoleAsync(role: new Role
        {
            Id = Guid.NewGuid(),
            AppId = app.Id,
            Name = Unique(prefix: "AcceptanceRole"),
            Description = "Acceptance role",
            Privs = string.Join(separator: ',',value: privileges),
        });

        await core.AddUserRoleAsync(userRole: new UserRole { RoleId = role.Id, UserId = "Guest" });

        return new SeededApp(app.Id, role.Id, app.Domain);
    }

    private async Task<int> DeleteAppAsync(string host, int id)
    {
        using WebAcceptanceFactory factory = new(new()
        {
            CoreConnectionString = fixture.Settings.CoreConnectionString,
            SsoConnectionString = fixture.Settings.SsoConnectionString,
            DecryptionKey = fixture.Settings.DecryptionKey,
            AggregateDomains = true,
        });

        using HttpClient client = factory.CreateClient(options: new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
        });

        using HttpRequestMessage request = new(HttpMethod.Delete, $"{BaseUrl}({id})");
        request.Headers.Host = host;

        using HttpResponseMessage response = await client.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return (int)response.StatusCode;
    }

    private async Task Teardown(SeededApp seededApp)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();

        using var core = scope.ServiceProvider
            .GetRequiredService<cCoder.Data.ICoreContextFactory>()
            .CreateCoreContext();

        await DeleteAllAsync(core: core,query: core.Set<FileContent>()
            .IgnoreQueryFilters()
            .Where(predicate: content => core.Set<DmsFile>()
            .IgnoreQueryFilters()
                .Any(predicate: file => file.Id == content.FileId
                    && core.Set<Folder>()
            .IgnoreQueryFilters()
                        .Any(predicate: folder => folder.Id == file.FolderId && folder.AppId == seededApp.AppId))));

        await DeleteAllAsync(core: core,query: core.Set<DmsFile>()
            .IgnoreQueryFilters()
            .Where(predicate: file => core.Set<Folder>()
            .IgnoreQueryFilters()
                .Any(predicate: folder => folder.Id == file.FolderId && folder.AppId == seededApp.AppId)));

        await DeleteAllAsync(core: core,query: core.Set<FolderRole>()
            .IgnoreQueryFilters()
            .Where(predicate: folderRole => folderRole.RoleId == seededApp.RoleId));

        await DeleteAllAsync(core: core,query: core.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(predicate: folder => folder.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<ScheduledTask>()
            .IgnoreQueryFilters()
            .Where(predicate: task => task.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<MailServer>()
            .IgnoreQueryFilters()
            .Where(predicate: server => server.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<QueuedEmail>()
            .IgnoreQueryFilters()
            .Where(predicate: email => email.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<SentEmail>()
            .IgnoreQueryFilters()
            .Where(predicate: email => email.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<AppFlowDefinition>()
            .IgnoreQueryFilters()
            .Where(predicate: flow => flow.AppId == seededApp.AppId));

        await DeleteAllAsync(core: core,query: core.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: userRole => userRole.RoleId == seededApp.RoleId));

        Role role = core.Set<Role>()
            .IgnoreQueryFilters()
            .FirstOrDefault(predicate: foundRole => foundRole.Id == seededApp.RoleId);

        if (role is not null)
        {
            await core.DeleteAsync(role: role);
        }

        App app = core.Set<App>()
            .IgnoreQueryFilters()
            .FirstOrDefault(predicate: foundApp => foundApp.Id == seededApp.AppId);

        if (app is not null)
        {
            await core.DeleteAsync(app: app);
        }
    }

    private static async Task DeleteAllAsync(
        CoreDataContext core,
        IQueryable query)
    {
        object[] items = query
            .Cast<object>()
            .ToArray();

        if (items.Length == 0)
        {
            return;
        }

        core.RemoveRange(entities: items);

        await core.SaveChangesAsync();
    }
}