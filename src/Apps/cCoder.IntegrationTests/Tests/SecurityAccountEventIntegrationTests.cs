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
public sealed partial class SecurityAccountEventIntegrationTests
{
    private const int BaselineAppId = 1;
    private const string AdminUserId = "admin";
    private const string DefaultPassword = "TestPass01!";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IntegrationAcceptanceFixture fixture;

    public SecurityAccountEventIntegrationTests(IntegrationAcceptanceFixture fixture) =>
        this.fixture = fixture;

    private async Task EnsureMailSenderAsync()
    {
        await using CoreDataContext core = CreateCoreContext();

        string sendUser = ReadMailSetting(
            "CCODER_MAIL_INTEGRATION_SEND_USER",
            "CCODER_MAIL_INTEGRATION_SMTP_USER");
        string from = TryReadMailSetting("CCODER_MAIL_INTEGRATION_SMTP_FROM") ?? sendUser;
        string sendHost = TryReadMailSetting("CCODER_MAIL_INTEGRATION_SEND_HOST") ?? "graph.microsoft.com";

        bool hasMailSender = await core.Set<MailSender>().IgnoreQueryFilters()
            .AnyAsync(mailSender =>
                mailSender.AppId == BaselineAppId
                && mailSender.Name == "Default");

        if (hasMailSender)
            return;

        await core.Set<MailSender>().AddAsync(new MailSender
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

    private async Task<(SSOUser User, string Token)> RegisterAsync(RegisterUser user, string authToken) =>
        await PostUserTokenResultAsync("/Api/Account/Register", user, authToken);

    private async Task<(SSOUser User, string Token)> InviteAsync(RegisterUser user, string authToken) =>
        await PostUserTokenResultAsync("/Api/Account/Invite", user, authToken);

    private async Task AcceptInviteAsync(string userId, string token, RegisterUser user)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
            $"/Api/Account/AcceptInvite?userId={WebUtility.UrlEncode(userId)}&inviteToken={WebUtility.UrlEncode(token)}&t={WebUtility.UrlEncode(token)}",
            user,
            JsonOptions);

        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "{0}",
            BuildFailureMessage(content));
    }

    private async Task ConfirmRegistrationAsync(string token)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsync(
            $"/Api/Account/ConfirmRegistration?confirmationToken={WebUtility.UrlEncode(token)}&t={WebUtility.UrlEncode(token)}",
            content: null);

        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "{0}",
            BuildFailureMessage(content));
    }

    private async Task<Token> LoginAsync(RegisterUser user)
    {
        using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
            "/Api/Account/Login",
            new Auth
            {
                User = user.Email,
                Pass = user.Password
            },
            JsonOptions);

        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);

        return JsonSerializer.Deserialize<Token>(content, JsonOptions)
            ?? throw new InvalidOperationException("Expected login token.");
    }

    private async Task<(SSOUser User, string Token)> PostUserTokenResultAsync(
        string relativeUrl,
        RegisterUser user,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, WithAuthToken(relativeUrl, authToken))
        {
            Content = JsonContent.Create(user, options: JsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(authToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", authToken);

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);

        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            throw new XunitException(BuildFailureMessage(content));

        using JsonDocument document = JsonDocument.Parse(content);

        SSOUser ssoUser = JsonSerializer.Deserialize<SSOUser>(
            document.RootElement.GetProperty("user").GetRawText(),
            JsonOptions)
            ?? throw new InvalidOperationException("Expected SSO user.");

        string token = document.RootElement.GetProperty("token").GetString();

        return (ssoUser, token);
    }

    private static string WithAuthToken(string relativeUrl, string authToken)
    {
        if (string.IsNullOrWhiteSpace(authToken))
            return relativeUrl;

        string separator = relativeUrl.Contains('?')
            ? "&"
            : "?";

        return $"{relativeUrl}{separator}t={WebUtility.UrlEncode(authToken)}";
    }

    private async Task AssertAppSecurityUserCreatedAsync(RegisterUser user, bool expectedIsActive = true)
    {
        await WaitUntilAsync(async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<CoreUser>().IgnoreQueryFilters()
                .AnyAsync(coreUser =>
                    coreUser.Email == user.Email
                    && coreUser.DisplayName == user.DisplayName
                    && coreUser.DefaultCultureId == user.Culture);
        });

        await using CoreDataContext verification = CreateCoreContext();

        CoreUser createdUser = await verification.Set<CoreUser>().IgnoreQueryFilters()
            .SingleAsync(coreUser => coreUser.Email == user.Email);

        createdUser.IsActive.Should().Be(expectedIsActive);

        bool hasUsersRole = await verification.Set<UserRole>().IgnoreQueryFilters()
            .AnyAsync(userRole =>
                userRole.UserId == createdUser.Id
                && verification.Set<Role>().IgnoreQueryFilters()
                    .Any(role =>
                        role.Id == userRole.RoleId
                        && role.AppId == BaselineAppId
                        && role.Name == "Users"));

        hasUsersRole.Should().BeTrue();
    }

    private async Task<QueuedEmail> AssertQueuedEmailAsync(RegisterUser user, string subjectFragment)
    {
        await WaitUntilAsync(async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<QueuedEmail>().IgnoreQueryFilters()
                .AnyAsync(email =>
                    email.AppId == BaselineAppId
                    && email.To == user.Email
                    && email.Subject.Contains(subjectFragment));
        });

        await using CoreDataContext verification = CreateCoreContext();

        return await verification.Set<QueuedEmail>().IgnoreQueryFilters()
            .OrderByDescending(email => email.Id)
            .FirstAsync(email =>
                email.AppId == BaselineAppId
                && email.To == user.Email
                && email.Subject.Contains(subjectFragment));
    }

    private async Task<SentEmail> AssertSentEmailAsync(RegisterUser user, string subjectFragment)
    {
        await WaitUntilAsync(async () =>
        {
            await using CoreDataContext core = CreateCoreContext();

            return await core.Set<SentEmail>().IgnoreQueryFilters()
                .AnyAsync(email =>
                    email.AppId == BaselineAppId
                    && email.To == user.Email
                    && email.Subject.Contains(subjectFragment));
        }, attempts: 240, delayMilliseconds: 1000);

        await using CoreDataContext verification = CreateCoreContext();

        return await verification.Set<SentEmail>().IgnoreQueryFilters()
            .OrderByDescending(email => email.Id)
            .FirstAsync(email =>
                email.AppId == BaselineAppId
                && email.To == user.Email
                && email.Subject.Contains(subjectFragment));
    }

    private async Task<ReceivedEmail> ReceiveEmailAsync(
        RegisterUser user,
        string subjectFragment,
        DateTimeOffset from)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(3);
        string receiveUser = ReadMailSetting(
            "CCODER_MAIL_INTEGRATION_RECEIVE_USER",
            "CCODER_MAIL_INTEGRATION_SEND_USER",
            "CCODER_MAIL_INTEGRATION_SMTP_USER");

        while (DateTimeOffset.UtcNow < deadline)
        {
            using HttpResponseMessage response = await fixture.WebClient.PostAsJsonAsync(
                "/Api/Core/ReceivedEmail/Receive",
                new MailboxReceiveRequest
                {
                    User = receiveUser,
                    From = from.AddMinutes(-1),
                    To = DateTimeOffset.UtcNow.AddMinutes(5),
                    MaximumMessages = 50,
                },
                JsonOptions);

            string content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK, BuildFailureMessage(content));

            ReceivedEmail[] receivedEmails = JsonSerializer.Deserialize<ReceivedEmail[]>(content, JsonOptions)
                ?? throw new InvalidOperationException("Expected received email payload.");

            ReceivedEmail receivedEmail = receivedEmails.FirstOrDefault(email =>
                email.Subject?.Contains(subjectFragment, StringComparison.OrdinalIgnoreCase) == true
                && email.ReceivedOn >= from.AddMinutes(-1));

            if (receivedEmail is not null)
                return receivedEmail;

            await Task.Delay(5000);
        }

        throw new TimeoutException($"Timed out waiting to receive '{subjectFragment}' email for {user.Email}.");
    }

    private static string ExtractTokenFromEmail(ReceivedEmail email)
    {
        string content = WebUtility.HtmlDecode(email.Content ?? string.Empty);
        Match match = Regex.Match(
            content,
            @"(?:[?&])t=([^""'<&\s]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (!match.Success)
            throw new InvalidOperationException($"Could not find a token link in email '{email.Subject}'.");

        return WebUtility.UrlDecode(match.Groups[1].Value);
    }

    private async Task CleanupAccountAsync(string email)
    {
        await using CoreDataContext core = CreateCoreContext();

        int[] queuedEmailIds = await core.Set<QueuedEmail>().IgnoreQueryFilters()
            .Where(queuedEmail => queuedEmail.To == email)
            .Select(queuedEmail => queuedEmail.Id)
            .ToArrayAsync();

        if (queuedEmailIds.Length > 0)
        {
            EmailSendFailure[] failures = await core.Set<EmailSendFailure>().IgnoreQueryFilters()
                .Where(failure => queuedEmailIds.Contains(failure.EmailId))
                .ToArrayAsync();

            core.Set<EmailSendFailure>().RemoveRange(failures);
            core.Set<QueuedEmail>().RemoveRange(
                await core.Set<QueuedEmail>().IgnoreQueryFilters()
                    .Where(queuedEmail => queuedEmailIds.Contains(queuedEmail.Id))
                    .ToArrayAsync());
        }

        core.Set<SentEmail>().RemoveRange(
            await core.Set<SentEmail>().IgnoreQueryFilters()
                .Where(sentEmail => sentEmail.To == email)
                .ToArrayAsync());
        core.Set<ReceivedEmail>().RemoveRange(
            await core.Set<ReceivedEmail>().IgnoreQueryFilters()
                .Where(receivedEmail => receivedEmail.To == email)
                .ToArrayAsync());

        await core.SaveChangesAsync();

        CoreUser[] coreUsers = await core.Set<CoreUser>().IgnoreQueryFilters()
            .Where(user => user.Email == email)
            .ToArrayAsync();

        if (coreUsers.Length > 0)
        {
            string[] coreUserIds = coreUsers.Select(user => user.Id).ToArray();
            UserRole[] userRoles = await core.Set<UserRole>().IgnoreQueryFilters()
                .Where(userRole => coreUserIds.Contains(userRole.UserId))
                .ToArrayAsync();

            core.Set<UserRole>().RemoveRange(userRoles);
            core.Set<CoreUser>().RemoveRange(coreUsers);
            await core.SaveChangesAsync();
        }

        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        SSOUser[] ssoUsers = await sso.Set<SSOUser>().IgnoreQueryFilters()
            .Where(user => user.Email == email)
            .ToArrayAsync();

        if (ssoUsers.Length == 0)
            return;

        string[] ssoUserIds = ssoUsers.Select(user => user.Id).ToArray();
        SsoToken[] tokens = await sso.Set<SsoToken>().IgnoreQueryFilters()
            .Where(token => ssoUserIds.Contains(token.UserName))
            .ToArrayAsync();
        SSOUserRole[] ssoUserRoles = await sso.Set<SSOUserRole>().IgnoreQueryFilters()
            .Where(userRole => ssoUserIds.Contains(userRole.UserId))
            .ToArrayAsync();
        UserEvent[] userEvents = await sso.Set<UserEvent>().IgnoreQueryFilters()
            .Where(userEvent => ssoUserIds.Contains(userEvent.CreatedBy))
            .ToArrayAsync();

        sso.Set<SsoToken>().RemoveRange(tokens);
        sso.Set<SSOUserRole>().RemoveRange(ssoUserRoles);
        sso.Set<UserEvent>().RemoveRange(userEvents);
        sso.Set<SSOUser>().RemoveRange(ssoUsers);
        await sso.SaveChangesAsync();
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>().CreateCoreContext();

    private string BuildFailureMessage(string content) =>
        $"""
        {content}

        Web output:
        {Tail(fixture.WebOutput)}

        HostedServices output:
        {Tail(fixture.HostedServicesOutput)}
        """;

    private static string Tail(string value, int length = 6000)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= length)
            return value ?? string.Empty;

        return value[^length..];
    }

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        string tokenId = Guid.NewGuid().ToString("N");

        sso.Add(new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private static RegisterUser CreateRegisterUser(string purpose) =>
        new()
        {
            DisplayName = $"Core {purpose} User",
            Email = ReadMailSetting(
                "CCODER_MAIL_INTEGRATION_TO",
                "CCODER_MAIL_INTEGRATION_RECEIVE_USER",
                "CCODER_MAIL_INTEGRATION_SEND_USER",
                "CCODER_MAIL_INTEGRATION_SMTP_USER"),
            Password = DefaultPassword,
            Culture = "en-GB",
            PhoneNumber = "01234567890",
            AppId = BaselineAppId,
            TenantId = "acceptance"
        };

    private static string ReadMailSetting(params string[] names)
    {
        string value = TryReadMailSetting(names);

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException(
            $"Missing mail integration environment variable. Checked: {string.Join(", ", names)}.");
    }

    private static string TryReadMailSetting(params string[] names)
    {
        foreach (string name in names)
        {
            string value =
                Environment.GetEnvironmentVariable(name)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        int attempts = 60,
        int delayMilliseconds = 500)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (await predicate())
                return;

            await Task.Delay(delayMilliseconds);
        }

        throw new TimeoutException("Timed out waiting for the expected condition.");
    }
}
