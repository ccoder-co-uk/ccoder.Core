using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AppEntity = cCoder.Data.Models.CMS.App;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class AppEventIntegrationTests
{
    [Fact]
    public async Task AppAdd_RaisesExternalEventAndHostedServicesCreatesChildren()
    {
        int appId = 0;
        string flowName = Unique("App Add Flow");
        string authToken = await CreateAuthTokenAsync(AdminUserId);

        try
        {
            await EnsureCultureAsync("en-GB", "English (UK)");

            AppEntity app = await PostAsJsonAsync<AppEntity>("/Api/Core/App", new
            {
                name = Unique("Integration App"),
                domain = $"{Unique("integration")}.local",
                defaultTheme = "Default",
                defaultCultureId = string.Empty,
                tenantId = Unique("tenant"),
                configJson = "{}",
                cultures = new[]
                {
                    new
                    {
                        cultureId = "en-GB"
                    }
                },
                folders = new[]
                {
                    new
                    {
                        name = "content"
                    }
                },
                mailServers = new[]
                {
                    new
                    {
                        name = "Integration SMTP",
                        user = "user",
                        password = "pass",
                        host = "smtp.example.com",
                        fromEmail = "noreply@example.com",
                        port = 25,
                        enableSSL = false
                    }
                },
                calendars = new[]
                {
                    new
                    {
                        name = "Integration Calendar",
                        description = "Calendar"
                    }
                },
                flows = new[]
                {
                    new
                    {
                        name = flowName,
                        description = "Integration flow",
                        definitionJson = SimpleFlowDefinitionJson,
                        configJson = "{}",
                        createdBy = "Guest",
                        createdOn = DateTimeOffset.UtcNow,
                        lastUpdatedBy = "Guest",
                        lastUpdated = DateTimeOffset.UtcNow
                    }
                }
            }, authToken: authToken);

            appId = app.Id;

            await WaitUntilAsync(async () =>
            {
                await using CoreDataContext core = CreateCoreContext();
                return await core.Set<Folder>().IgnoreQueryFilters().AnyAsync(folder => folder.AppId == appId && folder.Path == "content");
            });

            await using CoreDataContext verification = CreateCoreContext();
            (await verification.Set<Role>().IgnoreQueryFilters().CountAsync(role => role.AppId == appId)).Should().BeGreaterThanOrEqualTo(3);
            (await verification.Set<AppCulture>().IgnoreQueryFilters().AnyAsync(culture => culture.AppId == appId && culture.CultureId == "en-GB")).Should().BeTrue();
            (await verification.Set<Folder>().IgnoreQueryFilters().AnyAsync(folder => folder.AppId == appId && folder.Path == "content")).Should().BeTrue();
            (await verification.Set<MailServer>().IgnoreQueryFilters().AnyAsync(server => server.AppId == appId && server.Name == "Integration SMTP")).Should().BeTrue();
            (await verification.Set<Calendar>().IgnoreQueryFilters().AnyAsync(calendar => calendar.AppId == appId && calendar.Name == "Integration Calendar")).Should().BeTrue();
            (await verification.Set<FlowDefinition>().IgnoreQueryFilters().AnyAsync(flow => flow.AppId == appId && flow.Name == flowName)).Should().BeTrue();
        }
        finally
        {
            if (appId != 0)
                await DeleteAppGraphAsync(appId);
        }
    }
}
