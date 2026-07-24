// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        // Given
        await EnsureMailSenderAsync();
        RegisterUser user = CreateRegisterUser(purpose: "invitation");
        await CleanupAccountAsync(email: user.Email);

        DateTimeOffset requestedAt = DateTimeOffset.UtcNow;
        string authToken = await CreateAuthTokenAsync(userId: AdminUserId);

        // When
        (SSOUser invitedUser, string inviteToken) = await InviteAsync(user: user,authToken: authToken);

        // Then
        invitedUser.Email.Should()
            .Be(expected: user.Email);

        inviteToken.Should()
            .NotBeNullOrWhiteSpace();

        await AssertAppSecurityUserCreatedAsync(user: user,expectedIsActive: false);
        QueuedEmail queuedEmail = await AssertQueuedEmailAsync(user: user,subjectFragment: "Confirm Invitation");

        queuedEmail.To.Should()
            .Be(expected: user.Email);

        queuedEmail.Content.Should()
            .Contain(expected: inviteToken);

        await AssertSentEmailAsync(user: user,subjectFragment: "Confirm Invitation");

        ReceivedEmail receivedEmail = await ReceiveEmailAsync(
user:             user,subjectFragment:             "Confirm Invitation",from:             requestedAt);

        string emailToken = ExtractTokenFromEmail(email: receivedEmail);

        emailToken.Should()
            .Be(expected: inviteToken);

        await AcceptInviteAsync(userId: invitedUser.Id,token: emailToken,user: user);
        await AssertAppSecurityUserCreatedAsync(user: user);

        Token loginToken = await LoginAsync(user: user);

        loginToken.Id.Should()
            .NotBeNullOrWhiteSpace();
    }
}