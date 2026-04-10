using System.ComponentModel.DataAnnotations;
using cCoder.Core.Models;
using cCoder.Data.Models.Security;
using FluentAssertions;
using Moq;
using Xunit;


namespace cCoder.Core.Services.Tests.CMS.Orchestrations;

public partial class CMSUserRegistrationOrchestrationServiceTests
{
    [Fact]
    public async Task ShouldSaveRoleWhenUsersRoleExistsAndUserNotAssignedForInviteUserAsync()
    {
        User user = CreateUser();
        App app = CreateAppWithoutEmailInfrastructure(1);
        Role usersRole = new() { Id = Guid.NewGuid(), AppId = app.Id, Name = "Users", Users = [] };
        app.Roles = [usersRole];
        IQueryable<UserRole> existingUserRoles = Array.Empty<UserRole>().AsQueryable();

        contentManagementAppServiceMock.Setup(x => x.Get(app.Id, true)).Returns(app);
        roleOrchestrationServiceMock
            .Setup(x => x.GetAll(true))
            .Returns(new[] { usersRole }.AsQueryable());
        userOrchestrationServiceMock.Setup(x => x.AddAsync(user)).ReturnsAsync(user);
        userRoleOrchestrationServiceMock.Setup(x => x.GetAll(true)).Returns(existingUserRoles);
        userRoleOrchestrationServiceMock
            .Setup(x => x.SaveAsync(It.Is<UserRole>(ur => ur.RoleId == usersRole.Id && ur.UserId == user.Id)))
            .ReturnsAsync(new UserRole { RoleId = usersRole.Id, UserId = user.Id });

        User result = await orchestrationService.InviteUserAsync(user, app.Id, "token");

        result.Should().BeSameAs(user);
        contentManagementAppServiceMock.Verify(x => x.Get(app.Id, true), Times.Once);
        roleOrchestrationServiceMock.Verify(x => x.GetAll(true), Times.Once);
        userOrchestrationServiceMock.Verify(x => x.AddAsync(user), Times.Once);
        userRoleOrchestrationServiceMock.Verify(x => x.GetAll(true), Times.Once);
        userRoleOrchestrationServiceMock.Verify(
            x => x.SaveAsync(It.Is<UserRole>(ur => ur.RoleId == usersRole.Id && ur.UserId == user.Id)),
            Times.Once
        );
        templatedEmailOrchestrationServiceMock.VerifyNoOtherCalls();
    }

}






