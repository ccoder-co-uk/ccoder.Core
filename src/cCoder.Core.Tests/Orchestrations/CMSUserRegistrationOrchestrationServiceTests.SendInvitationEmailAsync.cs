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
    public async Task ShouldReturnWithoutQueueingWhenTemplateOrMailServerMissingForSendInvitationEmailAsync()
    {
        App app = CreateAppWithoutEmailInfrastructure(1);
        User user = CreateUser();

        await orchestrationService.SendInvitationEmailAsync("token", app, user);

        templatedEmailOrchestrationServiceMock.VerifyNoOtherCalls();
    }

}






