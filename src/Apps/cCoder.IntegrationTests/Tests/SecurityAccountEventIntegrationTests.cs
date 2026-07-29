// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.Data;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using cCoder.Mail.Models;
using cCoder.IntegrationTests.Infrastructure;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;
using CoreUser = cCoder.Data.Models.Security.User;
using SsoToken = cCoder.Security.Objects.Entities.Token;

namespace cCoder.IntegrationTests.Tests;

[Collection(IntegrationAcceptanceCollection.Name)]
public sealed partial class SecurityAccountEventIntegrationTests(IntegrationAcceptanceFixture fixture)
{
    private const int BaselineAppId = 1;
    private const string AdminUserId = "admin";
    private const string DefaultPassword = "TestPass01!";

    private static readonly Regex TokenLinkRegex = new(
        pattern: @"(?:[?&])t=([^""'<&\s]+)",
        options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IntegrationAcceptanceFixture fixture = fixture;

    private async Task EnsureMailSenderAsync()
    {
        await using CoreDataContext core = CreateCoreContext();

        string sendUser = fixture.Settings.MailSendUser;
        string from = sendUser;
        string sendHost = "graph.microsoft.com";

        bool hasMailSender = await core.Set<MailSender>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: mailSender =>
                mailSender.AppId == BaselineAppId
                && mailSender.Name == "Default");

        if (hasMailSender)
        {
            return;
        }

        await core.Set<MailSender>()
            .AddAsync(entity: new MailSender
        {
            AppId = BaselineAppId,
            Name = "Default",
            ProviderName = MailProviderNames.MicrosoftGraph,
            User = sendUser,
            Password = string.Empty,
            Host = sendHost,
            FromEmail = from,
            Port = 443,
            EnableSSL = true
        });

        await core.SaveChangesAsync();
    }

    private Task<(SSOUser User, string Token)> RegisterAsync(RegisterUser user, string authToken) =>
        PostUserTokenResultAsync(relativeUrl: "/Api/Account/Register",user: user,authToken: authToken);

    private Task<(SSOUser User, string Token)> InviteAsync(RegisterUser user, string authToken) =>
        PostUserTokenResultAsync(relativeUrl: "/Api/Account/Invite",user: user,authToken: authToken);

    private async Task AcceptInviteAsync(string userId, string token, RegisterUser user)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
requestUri:             $"/Api/Account/AcceptInvite?userId={WebUtility.UrlEncode(value: userId)}&inviteToken={WebUtility.UrlEncode(value: token)}&t={WebUtility.UrlEncode(value: token)}",value:             user,options:             JsonOptions);

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(
expected:             HttpStatusCode.OK,because:             "{0}",becauseArgs:             BuildFailureMessage(content: content));
    }

    private async Task ConfirmRegistrationAsync(string token)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsync(
requestUri:             $"/Api/Account/ConfirmRegistration?confirmationToken={WebUtility.UrlEncode(value: token)}&t={WebUtility.UrlEncode(value: token)}",            content: null);

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(
expected:             HttpStatusCode.OK,because:             "{0}",becauseArgs:             BuildFailureMessage(content: content));
    }

    private async Task<Token> LoginAsync(RegisterUser user)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
requestUri:             "/Api/Account/Login",value:             new Auth
            {
                User = user.Email,
                Pass = user.Password
            },options:             JsonOptions);

        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return JsonSerializer.Deserialize<Token>(json: content,options: JsonOptions)
            ?? throw new InvalidOperationException("Expected login token.");
    }

    private async Task<(SSOUser User, string Token)> PostUserTokenResultAsync(
        string relativeUrl,
        RegisterUser user,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, WithAuthToken(relativeUrl: relativeUrl,authToken: authToken))
        {
            Content = JsonContent.Create(inputValue: user,options: JsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(value: authToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", authToken);
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);

        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new XunitException(BuildFailureMessage(content: content));
        }

        using JsonDocument document = JsonDocument.Parse(json: content);

        SSOUser ssoUser = JsonSerializer.Deserialize<SSOUser>(
json:             document.RootElement.GetProperty(propertyName: "user")
            .GetRawText(),options:             JsonOptions)
            ?? throw new InvalidOperationException("Expected SSO user.");

        string token = document.RootElement.GetProperty(propertyName: "token")
            .GetString();

        return (ssoUser, token);
    }

    private static string WithAuthToken(string relativeUrl, string authToken)
    {
        if (string.IsNullOrWhiteSpace(value: authToken))
        {
            return relativeUrl;
        }

        string separator = relativeUrl.Contains(value: '?')
            ? "&"
            : "?";

        return $"{relativeUrl}{separator}t={WebUtility.UrlEncode(value: authToken)}";
    }

    private async Task AssertAppSecurityUserCreatedAsync(RegisterUser user, bool expectedIsActive = true)
    {
        await WaitUntilAsync(predicate: async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<CoreUser>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: coreUser =>
                    coreUser.Email == user.Email
                    && coreUser.DisplayName == user.DisplayName
                    && coreUser.DefaultCultureId == user.Culture);
        });

        await using CoreDataContext verification = CreateCoreContext();

        CoreUser createdUser = await verification.Set<CoreUser>()
            .IgnoreQueryFilters()
            .SingleAsync(predicate: coreUser => coreUser.Email == user.Email);

        createdUser.IsActive.Should()
            .Be(expected: expectedIsActive);

        try
        {
            await WaitUntilAsync(predicate: async () =>
            {
                await using CoreDataContext core = CreateCoreContext();

                return await core.Set<UserRole>()
                    .IgnoreQueryFilters()
                    .AnyAsync(predicate: userRole =>
                        userRole.UserId == createdUser.Id
                        && core.Set<Role>()
                    .IgnoreQueryFilters()
                            .Any(predicate: role =>
                                role.Id == userRole.RoleId
                                && role.AppId == BaselineAppId
                                && role.Name == "Users"));
            });
        }
        catch (TimeoutException exception)
        {
            await using CoreDataContext diagnosticContext = CreateCoreContext();

            string roleState = string.Join(
separator:                 Environment.NewLine,value:                 await diagnosticContext.Set<Role>()
                .IgnoreQueryFilters()
                    .Where(predicate: role => role.Name == "Users")
                    .Select(selector: role => $"Users role: {role.Id}, AppId={role.AppId}")
                    .ToArrayAsync());

            string assignmentState = string.Join(
separator:                 Environment.NewLine,value:                 await diagnosticContext.Set<UserRole>()
                .IgnoreQueryFilters()
                    .Where(predicate: userRole => userRole.UserId == createdUser.Id)
                    .Select(selector: userRole => $"User role assignment: UserId={userRole.UserId}, RoleId={userRole.RoleId}")
                    .ToArrayAsync());

            throw new TimeoutException(
                BuildFailureMessage(
content:                     $"Timed out waiting for the default Users role assignment.{Environment.NewLine}" +
                    $"{roleState}{Environment.NewLine}{assignmentState}"),
                exception);
        }
    }

    private async Task<QueuedEmail> AssertQueuedEmailAsync(RegisterUser user, string subjectFragment)
    {
        await WaitUntilAsync(predicate: async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<QueuedEmail>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: email =>
                    email.AppId == BaselineAppId
                    && email.To == user.Email
                    && email.Subject.Contains(value: subjectFragment));
        });

        await using CoreDataContext verification = CreateCoreContext();

        return await verification.Set<QueuedEmail>()
            .IgnoreQueryFilters()
            .OrderByDescending(keySelector: email => email.Id)
            .FirstAsync(predicate: email =>
                email.AppId == BaselineAppId
                && email.To == user.Email
                && email.Subject.Contains(value: subjectFragment));
    }

    private async Task<SentEmail> AssertSentEmailAsync(RegisterUser user, string subjectFragment)
    {
        await WaitUntilAsync(predicate: async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<SentEmail>()
                .IgnoreQueryFilters()
                .AnyAsync(predicate: email =>
                    email.AppId == BaselineAppId
                    && email.To == user.Email
                    && email.Subject.Contains(value: subjectFragment));
        },attempts: 240,delayMilliseconds: 1000);

        await using CoreDataContext verification = CreateCoreContext();

        return await verification.Set<SentEmail>()
            .IgnoreQueryFilters()
            .OrderByDescending(keySelector: email => email.Id)
            .FirstAsync(predicate: email =>
                email.AppId == BaselineAppId
                && email.To == user.Email
                && email.Subject.Contains(value: subjectFragment));
    }

    private async Task<ReceivedEmail> ReceiveEmailAsync(
        RegisterUser user,
        string subjectFragment,
        DateTimeOffset from)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(minutes: 3);

        string receiveUser = fixture.Settings.MailReceiveUser;

        while (DateTimeOffset.UtcNow < deadline)
        {
            using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
requestUri:                 "/Api/Mail/ReceivedEmail/Receive",value:                 new MailboxReceiveRequest
                {
                    User = receiveUser,
                    From = from.AddMinutes(minutes: -1),
                    To = DateTimeOffset.UtcNow.AddMinutes(minutes: 5),
                    MaximumMessages = 50,
                },options:                 JsonOptions);

            string content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should()
                .Be(expected: HttpStatusCode.OK,because: BuildFailureMessage(content: content));

            ReceivedEmail[] receivedEmails = JsonSerializer.Deserialize<ReceivedEmail[]>(json: content,options: JsonOptions)
                ?? throw new InvalidOperationException("Expected received email payload.");

            ReceivedEmail receivedEmail = receivedEmails.FirstOrDefault(predicate: email =>
                email.Subject?.Contains(value: subjectFragment,comparisonType: StringComparison.OrdinalIgnoreCase) == true
                && email.ReceivedOn >= from.AddMinutes(minutes: -1));

            if (receivedEmail is not null)
            {
                return receivedEmail;
            }

            await Task.Delay(millisecondsDelay: 5000);
        }

        throw new TimeoutException($"Timed out waiting to receive '{subjectFragment}' email for {user.Email}.");
    }

    private static string ExtractTokenFromEmail(ReceivedEmail email)
    {
        string content = WebUtility.HtmlDecode(value: email.Content ?? string.Empty);

        Match match = TokenLinkRegex
            .Match(input: content);

        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find a token link in email '{email.Subject}'.");
        }

        return WebUtility.UrlDecode(encodedValue: match.Groups[1].Value);
    }

    private async Task CleanupAccountAsync(string email)
    {
        await using CoreDataContext core = CreateCoreContext();

        int[] queuedEmailIds = await core.Set<QueuedEmail>()
            .IgnoreQueryFilters()
            .Where(predicate: queuedEmail => queuedEmail.To == email)
            .Select(selector: queuedEmail => queuedEmail.Id)
            .ToArrayAsync();

        if (queuedEmailIds.Length > 0)
        {
            EmailSendFailure[] failures = await core.Set<EmailSendFailure>()
                .IgnoreQueryFilters()
                .Where(predicate: failure => queuedEmailIds.Contains(value: failure.EmailId))
                .ToArrayAsync();

            core.Set<EmailSendFailure>()
                .RemoveRange(entities: failures);

            core.Set<QueuedEmail>()
                .RemoveRange(
entities:                 await core.Set<QueuedEmail>()
                .IgnoreQueryFilters()
                    .Where(predicate: queuedEmail => queuedEmailIds.Contains(value: queuedEmail.Id))
                    .ToArrayAsync());
        }

        core.Set<SentEmail>()
            .RemoveRange(
entities:             await core.Set<SentEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: sentEmail => sentEmail.To == email)
                .ToArrayAsync());

        core.Set<ReceivedEmail>()
            .RemoveRange(
entities:             await core.Set<ReceivedEmail>()
            .IgnoreQueryFilters()
                .Where(predicate: receivedEmail => receivedEmail.To == email)
                .ToArrayAsync());

        await core.SaveChangesAsync();

        CoreUser[] coreUsers = await core.Set<CoreUser>()
            .IgnoreQueryFilters()
            .Where(predicate: user => user.Email == email)
            .ToArrayAsync();

        if (coreUsers.Length > 0)
        {
            string[] coreUserIds = [.. coreUsers.Select(selector: user => user.Id)];

            UserRole[] userRoles = await core.Set<UserRole>()
                .IgnoreQueryFilters()
                .Where(predicate: userRole => coreUserIds.Contains(value: userRole.UserId))
                .ToArrayAsync();

            core.Set<UserRole>()
                .RemoveRange(entities: userRoles);

            core.Set<CoreUser>()
                .RemoveRange(entities: coreUsers);

            await core.SaveChangesAsync();
        }

        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        SSOUser[] ssoUsers = await sso.Set<SSOUser>()
            .IgnoreQueryFilters()
            .Where(predicate: user => user.Email == email)
            .ToArrayAsync();

        if (ssoUsers.Length == 0)
        {
            return;
        }

        string[] ssoUserIds = [.. ssoUsers.Select(selector: user => user.Id)];

        SsoToken[] tokens = await sso.Set<SsoToken>()
            .IgnoreQueryFilters()
            .Where(predicate: token => ssoUserIds.Contains(value: token.UserName))
            .ToArrayAsync();

        SSOUserRole[] ssoUserRoles = await sso.Set<SSOUserRole>()
            .IgnoreQueryFilters()
            .Where(predicate: userRole => ssoUserIds.Contains(value: userRole.UserId))
            .ToArrayAsync();

        UserEvent[] userEvents = await sso.Set<UserEvent>()
            .IgnoreQueryFilters()
            .Where(predicate: userEvent => ssoUserIds.Contains(value: userEvent.CreatedBy))
            .ToArrayAsync();

        sso.Set<SsoToken>()
            .RemoveRange(entities: tokens);

        sso.Set<SSOUserRole>()
            .RemoveRange(entities: ssoUserRoles);

        sso.Set<UserEvent>()
            .RemoveRange(entities: userEvents);

        sso.Set<SSOUser>()
            .RemoveRange(entities: ssoUsers);

        await sso.SaveChangesAsync();
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

    private string BuildFailureMessage(string content) =>
        $"""
        {content}

        Web output:
        {Tail(value: fixture.WebOutput)}

        HostedServices output:
        {Tail(value: fixture.HostedServicesOutput)}
        """;

    private static string Tail(string value, int length = 6000)
    {
        if (string.IsNullOrWhiteSpace(value: value) || value.Length <= length)
        {
            return value ?? string.Empty;
        }

        return value[^length..];
    }

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        string tokenId = Guid.NewGuid()
            .ToString(format: "N");

        sso.Add(entity: new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(hours: 1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private RegisterUser CreateRegisterUser(string purpose) =>
        new()
        {
            DisplayName = $"Core {purpose} User",
            Email = fixture.Settings.MailReceiveUser,
            Password = DefaultPassword,
            Culture = "en-GB",
            PhoneNumber = "01234567890",
            AppId = BaselineAppId,
            TenantId = "acceptance"
        };

    private static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        int attempts = 60,
        int delayMilliseconds = 500)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (await predicate())
            {
                return;
            }

            await Task.Delay(millisecondsDelay: delayMilliseconds);
        }

        throw new TimeoutException("Timed out waiting for the expected condition.");
    }
}