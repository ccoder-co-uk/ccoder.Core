using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DmsFile = cCoder.Data.Models.DMS.File;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class AppEventIntegrationTests
{
    [Fact]
    public async Task AppUpdate_RaisesExternalEventAndHostedServicesUpdatesChildren()
    {
        int appId = 0;
        Guid roleId = Guid.NewGuid();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        string appDomain = $"{Unique("update")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(appDomain);
            await GrantGuestAdminAsync(appId);
            await SeedAppUpdateScenarioAsync(appId, roleId, rootFolderId, childFolderId, fileId);

            await SendAsJsonAsync(
                HttpMethod.Put,
                $"/Api/Core/App({appId})",
                new
                {
                    id = appId,
                    name = Unique("Updated App"),
                    domain = appDomain,
                    defaultTheme = "Default",
                    defaultCultureId = string.Empty,
                    tenantId = Unique("tenant"),
                    configJson = "{}",
                    roles = new[]
                    {
                        new
                        {
                            id = roleId,
                            appId,
                            name = "Editors",
                            description = "Updated role",
                            privs = "app_read,folder_update"
                        }
                    },
                    cultures = new[]
                    {
                        new
                        {
                            appId,
                            cultureId = "fr-FR"
                        }
                    },
                    folders = new[]
                    {
                        new
                        {
                            id = rootFolderId,
                            appId,
                            name = "renamed",
                            path = "renamed"
                        }
                    }
                },
                host: appDomain);

            await WaitUntilAsync(async () =>
            {
                await using CoreDataContext core = CreateCoreContext();
                return await core.Set<Folder>().IgnoreQueryFilters()
                    .AnyAsync(folder => folder.Id == childFolderId && folder.Path == "renamed/child");
            });

            await using CoreDataContext verification = CreateCoreContext();
            (await verification.Set<Role>().IgnoreQueryFilters().SingleAsync(role => role.Id == roleId)).Privs.Should().Be("app_read,folder_update");
            (await verification.Set<AppCulture>().IgnoreQueryFilters().AnyAsync(culture => culture.AppId == appId && culture.CultureId == "fr-FR")).Should().BeTrue();
            (await verification.Set<AppCulture>().IgnoreQueryFilters().AnyAsync(culture => culture.AppId == appId && culture.CultureId == "en-GB")).Should().BeFalse();
            (await verification.Set<Folder>().IgnoreQueryFilters().SingleAsync(folder => folder.Id == rootFolderId)).Path.Should().Be("renamed");
            (await verification.Set<Folder>().IgnoreQueryFilters().SingleAsync(folder => folder.Id == childFolderId)).Path.Should().Be("renamed/child");
            (await verification.Set<DmsFile>().IgnoreQueryFilters().SingleAsync(file => file.Id == fileId)).Path.Should().Be("renamed/child/file.txt");
        }
        finally
        {
            if (appId != 0)
                await DeleteAppGraphAsync(appId);
        }
    }
}
