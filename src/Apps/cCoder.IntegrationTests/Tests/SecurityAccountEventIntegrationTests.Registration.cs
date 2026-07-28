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
    [Trait("Category", "ExternalIntegration")]
    public async Task Registration_SendsConfirmationEmailAndCompletesRegistration()
    {
        // Given
        await EnsureMailSenderAsync();
        RegisterUser user = CreateRegisterUser(purpose: "registration");
        await CleanupAccountAsync(email: user.Email);

        DateTimeOffset requestedAt = DateTimeOffset.UtcNow;
        string authToken = await CreateAuthTokenAsync(userId: AdminUserId);

        // When
        (SSOUser registeredUser, string confirmationToken) = await RegisterAsync(user: user,authToken: authToken);

        // Then
        registeredUser.Email.Should()
            .Be(expected: user.Email);

        confirmationToken.Should()
            .NotBeNullOrWhiteSpace();

        await AssertAppSecurityUserCreatedAsync(user: user);
        QueuedEmail queuedEmail = await AssertQueuedEmailAsync(user: user,subjectFragment: "Confirm Registration");

        queuedEmail.Content.Should()
            .Contain(expected: confirmationToken);

        await AssertSentEmailAsync(user: user,subjectFragment: "Confirm Registration");

        ReceivedEmail receivedEmail = await ReceiveEmailAsync(
user:             user,subjectFragment:             "Confirm Registration",from:             requestedAt);

        string emailToken = ExtractTokenFromEmail(email: receivedEmail);

        emailToken.Should()
            .Be(expected: confirmationToken);

        await ConfirmRegistrationAsync(token: emailToken);

        Token loginToken = await LoginAsync(user: user);

        loginToken.Id.Should()
            .NotBeNullOrWhiteSpace();
    }
}