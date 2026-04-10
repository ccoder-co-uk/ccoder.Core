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
namespace Web.AcceptanceTests.Tests.Api;

[Collection(WebAcceptanceCollection.Name)]
public sealed class AppDeleteCascadeTests(WebAcceptanceFixture fixture)
{
    private HttpClient Client { get; } = fixture.Client;
    private string BaseUrl { get; } = "/Api/Core/App";

    private sealed record SeededApp(int AppId, Guid RoleId, string Domain);

    [Fact]
    public async Task Delete_RemovesCrossDomainChildren()
    {
        SeededApp seededApp = await SeedDatabase(
            "app_delete",
            "folder_delete",
            "file_delete",
            "mailserver_delete",
            "queuedemail_delete",
            "sentemail_delete",
            "role_delete",
            "scheduledtask_delete",
            "userrole_delete",
            "flowdefinition_delete");

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

                await core.AddAppFlowDefinitionAsync(new AppFlowDefinition
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

                await core.AddScheduledTaskAsync(new ScheduledTask
                {
                    AppId = seededApp.AppId,
                    FlowId = flowId,
                    Name = "Acceptance Task",
                    Description = "Task",
                    ExecutionArgs = "{}",
                    ScheduleInTicks = TimeSpan.FromMinutes(5).Ticks,
                    CreatedBy = "Guest",
                    UpdatedBy = "Guest",
                    ExecuteAs = "Guest",
                    Created = DateTimeOffset.UtcNow,
                    LastUpdated = DateTimeOffset.UtcNow
                });

                await core.AddMailServerAsync(new MailServer
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

                await core.AddQueuedEmailAsync(new QueuedEmail
                {
                    AppId = seededApp.AppId,
                    SentByUserId = "Guest",
                    Subject = "Queued",
                    Content = "Queued content",
                    To = "guest@example.com",
                    CC = "",
                    MailServerName = "Acceptance Server"
                });

                await core.AddSentEmailAsync(new SentEmail
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

                await core.AddFolderAsync(new Folder
                {
                    Id = rootFolderId,
                    AppId = seededApp.AppId,
                    Name = "Root",
                    Path = "root"
                });

                await core.AddFolderAsync(new Folder
                {
                    Id = childFolderId,
                    AppId = seededApp.AppId,
                    ParentId = rootFolderId,
                    Name = "Child",
                    Path = "root/child"
                });

                await core.AddFolderRoleAsync(new FolderRole { FolderId = rootFolderId, RoleId = seededApp.RoleId });
                await core.AddFolderRoleAsync(new FolderRole { FolderId = childFolderId, RoleId = seededApp.RoleId });

                await core.AddDmsFileAsync(new DmsFile
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
            }

            int actualStatusCode = await DeleteAppAsync(seededApp.Domain, seededApp.AppId);

            using IServiceScope verificationScope = fixture.Factory.Services.CreateScope();
            using var verifyCore = verificationScope.ServiceProvider
                .GetRequiredService<cCoder.Data.ICoreContextFactory>()
                .CreateCoreContext();

            actualStatusCode.Should().Be(200);
            verifyCore.Set<App>().IgnoreQueryFilters().Any(app => app.Id == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<Role>().IgnoreQueryFilters().Any(role => role.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<UserRole>().IgnoreQueryFilters().Any(userRole => userRole.RoleId == seededApp.RoleId).Should().BeFalse();
            verifyCore.Set<Folder>().IgnoreQueryFilters().Any(folder => folder.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<DmsFile>().IgnoreQueryFilters().Any(file => file.FolderId == childFolderId).Should().BeFalse();
            verifyCore.Set<FileContent>().IgnoreQueryFilters().Any(content => content.FileId == fileId).Should().BeFalse();
            verifyCore.Set<MailServer>().IgnoreQueryFilters().Any(server => server.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<QueuedEmail>().IgnoreQueryFilters().Any(email => email.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<SentEmail>().IgnoreQueryFilters().Any(email => email.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<ScheduledTask>().IgnoreQueryFilters().Any(task => task.AppId == seededApp.AppId).Should().BeFalse();
            verifyCore.Set<AppFlowDefinition>().IgnoreQueryFilters().Any(flow => flow.AppId == seededApp.AppId).Should().BeFalse();
        }
        finally
        {
            await Teardown(seededApp);
        }
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<SeededApp> SeedDatabase(params string[] privileges)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();
        using var core = scope.ServiceProvider
            .GetRequiredService<cCoder.Data.ICoreContextFactory>()
            .CreateCoreContext();

        App app = await core.AddAppAsync(new App
            {
                Name = Unique("AcceptanceApp"),
                Domain = $"{Unique("acceptance")}.local",
                DefaultTheme = "Default",
                DefaultCultureId = string.Empty,
                TenantId = Unique("tenant"),
                ConfigJson = "{}",
            });

        Role role = await core.AddRoleAsync(new Role
            {
                Id = Guid.NewGuid(),
                AppId = app.Id,
                Name = Unique("AcceptanceRole"),
                Description = "Acceptance role",
                Privs = string.Join(',', privileges),
            });

        await core.AddUserRoleAsync(new UserRole { RoleId = role.Id, UserId = "Guest" });

        return new SeededApp(app.Id, role.Id, app.Domain);
    }

    private async Task<int> DeleteAppAsync(string host, int id)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, $"{BaseUrl}({id})");
        request.Headers.Host = host;

        using HttpResponseMessage response = await Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return (int)response.StatusCode;
    }

    private async Task Teardown(SeededApp seededApp)
    {
        using IServiceScope scope = fixture.Factory.Services.CreateScope();
        using var core = scope.ServiceProvider
            .GetRequiredService<cCoder.Data.ICoreContextFactory>()
            .CreateCoreContext();

        UserRole[] userRoles = core.Set<UserRole>().IgnoreQueryFilters()
            .Where(userRole => userRole.RoleId == seededApp.RoleId)
            .ToArray();

        if (userRoles.Length > 0)
        {
            await core.DeleteAllAsync(userRoles);
        }

        Role role = core.Set<Role>().IgnoreQueryFilters()
            .FirstOrDefault(foundRole => foundRole.Id == seededApp.RoleId);

        if (role is not null)
        {
            await core.DeleteAsync(role);
        }

        App app = core.Set<App>().IgnoreQueryFilters()
            .FirstOrDefault(foundApp => foundApp.Id == seededApp.AppId);

        if (app is not null)
        {
            await core.DeleteAsync(app);
        }
    }
}



