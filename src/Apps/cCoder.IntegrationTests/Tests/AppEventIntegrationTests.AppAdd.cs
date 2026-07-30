// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        // Given
        int appId = 0;
        string flowName = Unique(prefix: "App Add Flow");
        string authToken = await CreateAuthTokenAsync(userId: AdminUserId);

        try
        {
            await EnsureCultureAsync(cultureId: "en-GB",name: "English (UK)");

            // When
            AppEntity app = (AppEntity)await PostAsJsonAsync(
                relativeUrl: "/Api/ContentManagement/App",
                payload: new
            {
                name = Unique(prefix: "Integration App"),
                domain = $"{Unique(prefix: "integration")}.local",
                defaultTheme = "Default",
                defaultCultureId = string.Empty,
                tenantId = Unique(prefix: "tenant"),
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
                },
                responseType: typeof(AppEntity),
                authToken: authToken);

            appId = app.Id;

            await WaitUntilAsync(predicate: async () =>
            {
                await using CoreDataContext core = CreateCoreContext();

                return await core.Set<Role>()
                    .IgnoreQueryFilters()
                    .CountAsync(predicate: role => role.AppId == appId) >= 3
                    && await core.Set<AppCulture>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")
                    && await core.Set<Folder>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: folder => folder.AppId == appId && folder.Path == "content")
                    && await core.Set<MailServer>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: server => server.AppId == appId && server.Name == "Integration SMTP")
                    && await core.Set<Calendar>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: calendar => calendar.AppId == appId && calendar.Name == "Integration Calendar")
                    && await core.Set<FlowDefinition>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: flow => flow.AppId == appId && flow.Name == flowName);
            },
            diagnosticsFactory: () => BuildEventDiagnosticsAsync(
                appId: appId,
                rootFolderId: Guid.Empty,
                childFolderId: Guid.Empty,
                fileId: Guid.Empty));

            await using CoreDataContext verification = CreateCoreContext();

            // Then
            (await verification.Set<Role>()
                .IgnoreQueryFilters()
                .CountAsync(predicate: role => role.AppId == appId)).Should()
                .BeGreaterThanOrEqualTo(expected: 3);

            (await verification.Set<AppCulture>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")).Should()
                .BeTrue();

            (await verification.Set<Folder>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: folder => folder.AppId == appId && folder.Path == "content")).Should()
                .BeTrue();

            (await verification.Set<MailServer>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: server => server.AppId == appId && server.Name == "Integration SMTP")).Should()
                .BeTrue();

            (await verification.Set<Calendar>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: calendar => calendar.AppId == appId && calendar.Name == "Integration Calendar")).Should()
                .BeTrue();

            (await verification.Set<FlowDefinition>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: flow => flow.AppId == appId && flow.Name == flowName)).Should()
                .BeTrue();
        }
        finally
        {
            if (appId != 0)
            {
                await DeleteAppGraphAsync(appId: appId);
            }
        }
    }
}