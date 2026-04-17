using FluentAssertions;
using Web.Services.Setup;
using Xunit;

namespace Web.Tests.Setup;

public sealed class SetupRequestHostNormalizerTests
{
    [Theory]
    [InlineData("www.Example.com", "example.com")]
    [InlineData(" LOCALHOST ", "localhost")]
    [InlineData("tenant.example.com", "tenant.example.com")]
    [InlineData(null, "")]
    public void ShouldNormalizeIncomingHostNames(string host, string expected)
    {
        string actual = SetupRequestHostNormalizer.Normalize(host);

        actual.Should().Be(expected);
    }
}
