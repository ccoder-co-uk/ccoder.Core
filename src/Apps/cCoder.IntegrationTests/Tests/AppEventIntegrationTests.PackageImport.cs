// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.Data.Models.Security;
using cCoder.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class AppEventIntegrationTests
{
    [Fact]
    public async Task PackageImport_ProcessesInWebAndPersistsRootPageRole()
    {
        // Given
        int appId = 0;
        string appDomain = $"{Unique(prefix: "package")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(domain: appDomain);
            await GrantGuestAdminAsync(appId: appId);
            await SeedRootPageAndGuestRoleAsync(appId: appId);

            Package package = CreateRootPageRolePackage();

            // When
            await SendAsJsonAsync(
                method: HttpMethod.Post,
                relativeUrl: $"/Api/Packaging/Package/Import?appId={appId}",
                payload: package,
                host: appDomain,
                expectedStatusCode: HttpStatusCode.Accepted);

            // Then
            await WaitUntilAsync(
                predicate: async () =>
                {
                    await using CoreDataContext core = CreateCoreContext();

                    return await core.Set<PageRole>()
                        .IgnoreQueryFilters()
                        .AnyAsync(predicate: pageRole =>
                            pageRole.Page.AppId == appId
                            && pageRole.Page.Path == string.Empty
                            && pageRole.Role.AppId == appId
                            && pageRole.Role.Name == "Guests");
                });
        }
        finally
        {
            if (appId != 0)
            {
                await DeleteAppGraphAsync(appId: appId);
            }
        }
    }

    private async Task SeedRootPageAndGuestRoleAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        _ = await core.AddPageAsync(page: new Page
        {
            AppId = appId,
            Path = string.Empty,
            Name = "Home",
            ResourceKey = "Core",
        });

        Role guests = await core.AddRoleAsync(role: new Role
        {
            Id = Guid.NewGuid(),
            AppId = appId,
            Name = "Guests",
            Privs = string.Empty,
        });

        _ = await core.AddUserRoleAsync(userRole: new UserRole
        {
            RoleId = guests.Id,
            UserId = "Guest",
        });
    }

    private static Package CreateRootPageRolePackage() =>
        new()
        {
            Name = "Root page role",
            Items =
            [
                new PackageItem
                {
                    Type = "ContentManagement/Page",
                    Data = JsonSerializer.Serialize(value: new[]
                    {
                        new
                        {
                            Path = string.Empty,
                            Name = "Home",
                            ResourceKey = "Core",
                            ShowOnMenus = false,
                            Order = 1,
                        },
                    }),
                },
                new PackageItem
                {
                    Type = "AppSecurity/Role",
                    Data = JsonSerializer.Serialize(value: new[]
                    {
                        new
                        {
                            Name = "Guests",
                            Privs = string.Empty,
                        },
                    }),
                },
                new PackageItem
                {
                    Type = "ContentManagement/PageRole",
                    Data = JsonSerializer.Serialize(value: new[]
                    {
                        new
                        {
                            Path = string.Empty,
                            Role = "Guests",
                        },
                    }),
                },
            ],
        };
}