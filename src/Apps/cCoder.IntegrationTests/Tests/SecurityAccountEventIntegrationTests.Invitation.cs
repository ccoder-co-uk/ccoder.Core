using cCoder.Data.Models.Mail;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class SecurityAccountEventIntegrationTests
{
    [Fact]
    public async Task Invitation_CreatesAppUserQueuesInvitationEmailAndAllowsAcceptedLogin()
    {
        await EnsureMailSenderAsync();
        RegisterUser user = CreateRegisterUser("invitation");
        await CleanupAccountAsync(user.Email);

        DateTimeOffset requestedAt = DateTimeOffset.UtcNow;
        string authToken = await CreateAuthTokenAsync(AdminUserId);

        (SSOUser invitedUser, string inviteToken) = await InviteAsync(user, authToken);

        invitedUser.Email.Should().Be(user.Email);
        inviteToken.Should().NotBeNullOrWhiteSpace();

        await AssertAppSecurityUserCreatedAsync(user, expectedIsActive: false);
        QueuedEmail queuedEmail = await AssertQueuedEmailAsync(user, "Confirm Invitation");

        queuedEmail.To.Should().Be(user.Email);
        queuedEmail.Content.Should().Contain(inviteToken);

        await AssertSentEmailAsync(user, "Confirm Invitation");
        ReceivedEmail receivedEmail = await ReceiveEmailAsync(
            user,
            "Confirm Invitation",
            requestedAt);
        string emailToken = ExtractTokenFromEmail(receivedEmail);

        emailToken.Should().Be(inviteToken);

        await AcceptInviteAsync(invitedUser.Id, emailToken, user);
        await AssertAppSecurityUserCreatedAsync(user);

        Token loginToken = await LoginAsync(user);

        loginToken.Id.Should().NotBeNullOrWhiteSpace();
    }
}
