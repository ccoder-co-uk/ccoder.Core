using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using FluentAssertions;
using Moq;
using Xunit;
using AppSecurityAppOrchestrationService = cCoder.AppSecurity.Services.Orchestrations.IAppOrchestrationService;
using AppSecurityUserRoleBroker = cCoder.AppSecurity.Brokers.Storages.IUserRoleBroker;

namespace cCoder.Core.Tests;

public sealed class HostedServicesAppSecurityAppAddOrchestrationServiceTests
{
    [Fact]
    public async Task HandleAsync_ShouldAddAppAndPersistDistinctRoleUsers()
    {
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
            .Setup(broker => broker.GetAllUserRoles(true))
            .Returns(Array.Empty<UserRole>().AsQueryable());
        userRoleBrokerMock
            .Setup(broker => broker.AddUserRoleAsync(It.IsAny<UserRole>()))
            .ReturnsAsync((UserRole userRole) => userRole);

        await service.HandleAsync(app);

        appOrchestrationServiceMock.Verify(service => service.AddAsync(app), Times.Once);
        userRoleBrokerMock.Verify(
            broker => broker.AddUserRoleAsync(It.Is<UserRole>(userRole =>
                userRole.RoleId == administratorRoleId &&
                userRole.UserId == "Paul")),
            Times.Once);
        userRoleBrokerMock.Verify(
            broker => broker.AddUserRoleAsync(It.Is<UserRole>(userRole =>
                userRole.RoleId == guestRoleId &&
                userRole.UserId == "Guest")),
            Times.Once);
        userRoleBrokerMock.Verify(
            broker => broker.AddUserRoleAsync(It.IsAny<UserRole>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_ShouldSkipEmptyRoleUsers()
    {
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
            .Setup(broker => broker.GetAllUserRoles(true))
            .Returns(Array.Empty<UserRole>().AsQueryable());

        await service.HandleAsync(app);

        appOrchestrationServiceMock.Verify(service => service.AddAsync(app), Times.Once);
        userRoleBrokerMock.Verify(
            broker => broker.AddUserRoleAsync(It.IsAny<UserRole>()),
            Times.Never);
    }
}
