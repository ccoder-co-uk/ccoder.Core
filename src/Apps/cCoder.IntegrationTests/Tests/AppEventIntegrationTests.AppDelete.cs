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
        int appId = 0;
        Guid roleId = Guid.NewGuid();
        Guid flowId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        string appDomain = $"{Unique("delete")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(appDomain);
            await GrantGuestAdminAsync(appId);
            await SeedAppDeleteScenarioAsync(appId, roleId, flowId, folderId, fileId);

            await SendWithOptionalHostAsync(HttpMethod.Delete, $"/Api/Core/App({appId})", host: appDomain);

            await WaitUntilAsync(async () =>
            {
                await using CoreDataContext core = CreateCoreContext();
                return !await core.Set<Role>().IgnoreQueryFilters().AnyAsync(role => role.AppId == appId);
            });

            await using CoreDataContext verification = CreateCoreContext();
            (await verification.Set<AppEntity>().IgnoreQueryFilters().AnyAsync(app => app.Id == appId)).Should().BeFalse();
            (await verification.Set<Role>().IgnoreQueryFilters().AnyAsync(role => role.AppId == appId)).Should().BeFalse();
            (await verification.Set<UserRole>().IgnoreQueryFilters().AnyAsync(userRole => userRole.RoleId == roleId)).Should().BeFalse();
            (await verification.Set<Folder>().IgnoreQueryFilters().AnyAsync(folder => folder.AppId == appId)).Should().BeFalse();
            (await verification.Set<DmsFile>().IgnoreQueryFilters().AnyAsync(file => file.Id == fileId)).Should().BeFalse();
            (await verification.Set<FileContent>().IgnoreQueryFilters().AnyAsync(content => content.FileId == fileId)).Should().BeFalse();
            (await verification.Set<MailServer>().IgnoreQueryFilters().AnyAsync(server => server.AppId == appId)).Should().BeFalse();
            (await verification.Set<Calendar>().IgnoreQueryFilters().AnyAsync(calendar => calendar.AppId == appId)).Should().BeFalse();
            (await verification.Set<FlowDefinition>().IgnoreQueryFilters().AnyAsync(flow => flow.Id == flowId)).Should().BeFalse();
        }
        finally
        {
            if (appId != 0)
                await DeleteAppGraphAsync(appId);
        }
    }
}
