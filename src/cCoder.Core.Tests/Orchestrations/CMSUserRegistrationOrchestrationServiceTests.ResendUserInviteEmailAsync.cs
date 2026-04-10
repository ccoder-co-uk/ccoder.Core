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
    public async Task ShouldThrowValidationExceptionWhenUserIsMissingForResendUserInviteEmailAsync()
    {
        App app = CreateAppWithoutEmailInfrastructure(1);
        userOrchestrationServiceMock.Setup(x => x.GetAll(true)).Returns(Array.Empty<User>().AsQueryable());
        contentManagementAppServiceMock.Setup(x => x.Get(app.Id, true)).Returns(app);

        Func<Task> act = async () =>
            await orchestrationService.ResendUserInviteEmailAsync("missing", app.Id, "token");

        await act.Should().ThrowAsync<ValidationException>().WithMessage("User not found");
    }

    [Fact]
    public async Task ShouldThrowValidationExceptionWhenAppIsMissingForResendUserInviteEmailAsync()
    {
        User user = CreateUser();
        userOrchestrationServiceMock
            .Setup(x => x.GetAll(true))
            .Returns(new[] { user }.AsQueryable());
        contentManagementAppServiceMock.Setup(x => x.Get(1, true)).Returns((App)null!);

        Func<Task> act = async () =>
            await orchestrationService.ResendUserInviteEmailAsync(user.Id, 1, "token");

        await act.Should().ThrowAsync<ValidationException>().WithMessage("App not found");
    }

}






