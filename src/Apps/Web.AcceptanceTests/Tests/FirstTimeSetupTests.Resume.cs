// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
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
        // Given
        await using SetupHarness harness = await SetupHarness.CreateAsync();

        ITenantManager tenantManager = harness.Factory.Services.GetRequiredService<ITenantManager>();

        await tenantManager.SetupAsync(
setupDetails:             new SetupDetails
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

        // When
        await SubmitSetupAsync(harness: harness);

        using HttpResponseMessage response = await harness.Client.GetAsync(requestUri: "/Setup");

        // Then
        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.Redirect);

        response.Headers.Location!.OriginalString.Should()
            .Be(expected: "/");
    }
}