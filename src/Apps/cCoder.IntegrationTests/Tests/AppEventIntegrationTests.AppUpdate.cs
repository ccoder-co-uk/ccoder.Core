// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        // Given
        int appId = 0;
        Guid roleId = Guid.NewGuid();
        Guid rootFolderId = Guid.NewGuid();
        Guid childFolderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        string appDomain = $"{Unique(prefix: "update")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(domain: appDomain);
            await GrantGuestAdminAsync(appId: appId);
            await SeedAppUpdateScenarioAsync(appId: appId,roleId: roleId,rootFolderId: rootFolderId,childFolderId: childFolderId,fileId: fileId);

            // When
            await SendAsJsonAsync(
method:                 HttpMethod.Put,relativeUrl:                 $"/Api/ContentManagement/App({appId})",payload:                 new
                {
                    id = appId,
                    name = Unique(prefix: "Updated App"),
                    domain = appDomain,
                    defaultTheme = "Default",
                    defaultCultureId = string.Empty,
                    tenantId = Unique(prefix: "tenant"),
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
                },                host: appDomain);

            await WaitUntilAsync(predicate: async () =>
            {
                await using CoreDataContext core = CreateCoreContext();

                return await core.Set<Folder>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: folder => folder.Id == childFolderId && folder.Path == "renamed/child");
            });

            await using CoreDataContext verification = CreateCoreContext();

            // Then
            (await verification.Set<Role>()
                .IgnoreQueryFilters()
                .SingleAsync(predicate: role => role.Id == roleId)).Privs.Should()
                .Be(expected: "app_read,folder_update");

            (await verification.Set<AppCulture>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: culture => culture.AppId == appId && culture.CultureId == "fr-FR")).Should()
                .BeTrue();

            (await verification.Set<AppCulture>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: culture => culture.AppId == appId && culture.CultureId == "en-GB")).Should()
                .BeFalse();

            (await verification.Set<Folder>()
                .IgnoreQueryFilters()
                .SingleAsync(predicate: folder => folder.Id == rootFolderId)).Path.Should()
                .Be(expected: "renamed");

            (await verification.Set<Folder>()
                .IgnoreQueryFilters()
                .SingleAsync(predicate: folder => folder.Id == childFolderId)).Path.Should()
                .Be(expected: "renamed/child");

            (await verification.Set<DmsFile>()
                .IgnoreQueryFilters()
                .SingleAsync(predicate: file => file.Id == fileId)).Path.Should()
                .Be(expected: "renamed/child/file.txt");
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