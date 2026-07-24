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
    public async Task Registration_SendsConfirmationEmailAndCompletesRegistration()
    {
        await EnsureMailSenderAsync();
        RegisterUser user = CreateRegisterUser("registration");
        await CleanupAccountAsync(user.Email);

        DateTimeOffset requestedAt = DateTimeOffset.UtcNow;
        string authToken = await CreateAuthTokenAsync(AdminUserId);

        (SSOUser registeredUser, string confirmationToken) = await RegisterAsync(user, authToken);

        registeredUser.Email.Should().Be(user.Email);
        confirmationToken.Should().NotBeNullOrWhiteSpace();

        await AssertAppSecurityUserCreatedAsync(user);
        QueuedEmail queuedEmail = await AssertQueuedEmailAsync(user, "Confirm Registration");

        queuedEmail.Content.Should().Contain(confirmationToken);

        await AssertSentEmailAsync(user, "Confirm Registration");
        ReceivedEmail receivedEmail = await ReceiveEmailAsync(
            user,
            "Confirm Registration",
            requestedAt);
        string emailToken = ExtractTokenFromEmail(receivedEmail);

        emailToken.Should().Be(confirmationToken);

        await ConfirmRegistrationAsync(emailToken);

        Token loginToken = await LoginAsync(user);

        loginToken.Id.Should().NotBeNullOrWhiteSpace();
    }
}