// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using FluentAssertions;
using Moq;
using Xunit;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using AppSecurityUserRoleBroker = cCoder.AppSecurity.Brokers.Storages.IUserRoleBroker;

namespace cCoder.Core.Tests;

public sealed partial class HostedServicesAppSecurityAppAddOrchestrationServiceTests
{
    [Fact]
    public async Task HandleAsync_ShouldAddAppAndPersistDistinctRoleUsers()
    {
        // Given
        Mock<AppSecurityAppOrchestrationService> appOrchestrationServiceMock = new();
        Mock<AppSecurityUserRoleBroker> userRoleBrokerMock = new();

        var service = new HostedServicesAppSecurityAppAddOrchestrationService(
            appOrchestrationServiceMock.Object,
            userRoleBrokerMock.Object);

        Guid administratorRoleId = Guid.NewGuid();
        Guid guestRoleId = Guid.NewGuid();

        App app = new()
        {
            Id = 42,
            Roles =
            [
                new Role
                {
                    Id = administratorRoleId,
                    Users =
                    [
                        new UserRole { RoleId = administratorRoleId, UserId = "Paul" },
                        new UserRole { RoleId = administratorRoleId, UserId = "Paul" }
                    ]
                },
                new Role
                {
                    Id = guestRoleId,
                    Users =
                    [
                        new UserRole { RoleId = guestRoleId, UserId = "Guest" }
                    ]
                }
            ]
        };

        userRoleBrokerMock
            .Setup(expression: broker => broker.GetAllUserRoles(ignoreFilters: true))
            .Returns(value: Array.Empty<UserRole>()
            .AsQueryable());

        userRoleBrokerMock
            .Setup(expression: broker => broker.AddUserRoleAsync(entity: It.IsAny<UserRole>()))
            .ReturnsAsync(valueFunction: (UserRole userRole) => userRole);

        // When
        await service.HandleAppAsync(app: app);

        // Then
        appOrchestrationServiceMock.Verify(
            expression: service => service.AddAppAsync(app: app),
            times: Times.Once);

        userRoleBrokerMock.Verify(
expression:             broker => broker.AddUserRoleAsync(entity: It.Is<UserRole>(match: userRole =>
                userRole.RoleId == administratorRoleId &&
                userRole.UserId == "Paul")),times:             Times.Once);

        userRoleBrokerMock.Verify(
expression:             broker => broker.AddUserRoleAsync(entity: It.Is<UserRole>(match: userRole =>
                userRole.RoleId == guestRoleId &&
                userRole.UserId == "Guest")),times:             Times.Once);

        userRoleBrokerMock.Verify(
expression:             broker => broker.AddUserRoleAsync(entity: It.IsAny<UserRole>()),times:             Times.Exactly(callCount: 2));
    }

    [Fact]
    public async Task HandleAsync_ShouldSkipEmptyRoleUsers()
    {
        // Given
        Mock<AppSecurityAppOrchestrationService> appOrchestrationServiceMock = new();
        Mock<AppSecurityUserRoleBroker> userRoleBrokerMock = new();

        var service = new HostedServicesAppSecurityAppAddOrchestrationService(
            appOrchestrationServiceMock.Object,
            userRoleBrokerMock.Object);

        App app = new()
        {
            Id = 42,
            Roles =
            [
                new Role { Id = Guid.NewGuid(), Users = [] },
                new Role
                {
                    Id = Guid.Empty,
                    Users =
                    [
                        new UserRole { RoleId = Guid.Empty, UserId = "Guest" },
                        new UserRole { RoleId = Guid.NewGuid(), UserId = string.Empty }
                    ]
                }
            ]
        };

        userRoleBrokerMock
            .Setup(expression: broker => broker.GetAllUserRoles(ignoreFilters: true))
            .Returns(value: Array.Empty<UserRole>()
            .AsQueryable());

        // When
        await service.HandleAppAsync(app: app);

        // Then
        appOrchestrationServiceMock.Verify(
            expression: service => service.AddAppAsync(app: app),
            times: Times.Once);

        userRoleBrokerMock.Verify(
expression:             broker => broker.AddUserRoleAsync(entity: It.IsAny<UserRole>()),times:             Times.Never);
    }
}