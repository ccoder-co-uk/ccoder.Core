// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class HostedServicesAppSecurityAppAddOrchestrationServiceTests
{
    [Fact]
    public async Task HandleAppAsyncShouldAddAppAndSaveUserRoles()
    {
        // Given
        Mock<IAppSecurityAppService> appSecurityAppServiceMock =
            new();

        Mock<IAppSecurityUserRoleService>
            appSecurityUserRoleServiceMock =
                new();

        IHostedServicesAppSecurityAppAddOrchestrationService service =
            new HostedServicesAppSecurityAppAddOrchestrationService(
                appSecurityAppService:
                    appSecurityAppServiceMock.Object,
                appSecurityUserRoleService:
                    appSecurityUserRoleServiceMock.Object);

        App app = new()
        {
            Id = 42
        };

        // When
        await service.HandleAppAsync(app: app);

        // Then
        appSecurityAppServiceMock.Verify(
            expression: dependency =>
                dependency.AddAppAsync(newApp: app),
            times: Times.Once);

        appSecurityUserRoleServiceMock.Verify(
            expression: dependency =>
                dependency.SaveAppUserRolesAsync(app: app),
            times: Times.Once);
    }
}