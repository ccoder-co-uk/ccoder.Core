using cCoder.Security.Data.Models;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Web.AcceptanceTests.Tests;

public sealed partial class FirstTimeSetupTests
{
    [Fact]
    public async Task ShouldResumeWhenSecurityTenantAlreadyExists()
    {
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        ITenantManager tenantManager = harness.Factory.Services.GetRequiredService<ITenantManager>();

        await tenantManager.SetupAsync(
            new SetupDetails
            {
                Tenant = new Tenant
                {
                    Id = "acceptance-platform",
                    Name = "Acceptance Platform",
                    Description = "Acceptance Platform tenant",
                    CreatedBy = "admin",
                    LastUpdatedBy = "admin",
                    CreatedOn = DateTimeOffset.UtcNow,
                    LastUpdated = DateTimeOffset.UtcNow,
                },
                User = new SSOUser
                {
                    Id = "admin",
                    DisplayName = "Acceptance Admin",
                    Email = "admin@localhost",
                    PasswordHash = "Password123!",
                }
            });

        await SubmitSetupAsync(harness);

        using HttpResponseMessage response = await harness.Client.GetAsync("/");
        string content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        content.Should().Contain("Welcome to cCoder");
    }
}
