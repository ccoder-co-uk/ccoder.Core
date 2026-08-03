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
using DmsFile = cCoder.Data.Models.DMS.File;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class AppEventIntegrationTests
{
    [Fact]
    public async Task AppDelete_RaisesExternalEventAndHostedServicesRemovesCrossDomainChildren()
    {
        // Given
        int appId = 0;
        Guid roleId = Guid.NewGuid();
        Guid flowId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        string appDomain = $"{Unique(prefix: "delete")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(domain: appDomain);
            await GrantGuestAdminAsync(appId: appId);
            await SeedAppDeleteScenarioAsync(appId: appId,roleId: roleId,flowId: flowId,folderId: folderId,fileId: fileId);

            // When
            await SendWithOptionalHostAsync(method: HttpMethod.Delete,relativeUrl: $"/Api/ContentManagement/App({appId})",host: appDomain);

            await WaitUntilAsync(
                predicate: async () =>
                {
                    await using CoreDataContext core = CreateCoreContext();

                    return !await core.Set<AppEntity>()
                        .IgnoreQueryFilters()
                        .AnyAsync(predicate: app => app.Id == appId);
                },
                diagnosticsFactory: () => Task.FromResult(result: $"""
                    Web output:
                    {ImportantLines(value: fixture.WebOutput)}

                    Hosted Services output:
                    {ImportantLines(value: fixture.HostedServicesOutput)}
                    """));

            await using CoreDataContext verification = CreateCoreContext();

            // Then
            (await verification.Set<AppEntity>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: app => app.Id == appId)).Should()
                .BeFalse();

            (await verification.Set<Role>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: role => role.AppId == appId)).Should()
                .BeFalse();

            (await verification.Set<UserRole>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: userRole => userRole.RoleId == roleId)).Should()
                .BeFalse();

            (await verification.Set<Folder>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: folder => folder.AppId == appId)).Should()
                .BeFalse();

            (await verification.Set<DmsFile>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: file => file.Id == fileId)).Should()
                .BeFalse();

            (await verification.Set<FileContent>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: content => content.FileId == fileId)).Should()
                .BeFalse();

            (await verification.Set<MailServer>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: server => server.AppId == appId)).Should()
                .BeFalse();

            (await verification.Set<Calendar>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: calendar => calendar.AppId == appId)).Should()
                .BeFalse();

            (await verification.Set<FlowDefinition>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: flow => flow.Id == flowId)).Should()
                .BeFalse();
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