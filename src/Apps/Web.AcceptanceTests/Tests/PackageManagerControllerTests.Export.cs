using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;

namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class PackageManagerControllerTests
{
    [Fact]
    public async Task ShouldReturnSeededPackagesWhenExport()
    {
        Package[] expectedPackages = AcceptanceSeedData.LoadExportPackages();

        IReadOnlyList<Package> actualPackages = await ExportPackagesAsync(1);

        actualPackages.Should().HaveCountGreaterThan(5);
        actualPackages
            .Select(package => package.Name)
            .Should()
            .Contain(expectedPackages.Select(package => package.Name).Distinct());
    }
}

