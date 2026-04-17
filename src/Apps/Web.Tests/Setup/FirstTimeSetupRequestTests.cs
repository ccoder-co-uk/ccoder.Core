using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Web.Models;
using Xunit;

namespace Web.Tests.Setup;

public sealed class FirstTimeSetupRequestTests
{
    [Fact]
    public void ShouldRequireMatchingPasswords()
    {
        FirstTimeSetupRequest model = new()
        {
            TenantName = "Demo Tenant",
            DisplayName = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
        };

        ValidationResult[] results = Validate(model);

        results.Should().Contain(result => result.MemberNames.Contains(nameof(FirstTimeSetupRequest.ConfirmPassword)));
    }

    [Fact]
    public void ShouldAcceptMinimalValidSetupDetails()
    {
        FirstTimeSetupRequest model = new()
        {
            TenantName = "Demo Tenant",
            DisplayName = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
        };

        ValidationResult[] results = Validate(model);

        results.Should().BeEmpty();
    }

    private static ValidationResult[] Validate(FirstTimeSetupRequest model)
    {
        List<ValidationResult> results = [];
        ValidationContext context = new(model);

        _ = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return [.. results];
    }
}
