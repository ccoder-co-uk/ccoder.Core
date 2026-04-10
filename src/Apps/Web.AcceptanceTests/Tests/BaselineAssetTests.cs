using System.Text.Json;
using FluentAssertions;
using Web.AcceptanceTests.Infrastructure;
using Xunit;


namespace Web.AcceptanceTests.Tests;

public sealed class BaselineAssetTests
{
    [Theory]
    [InlineData("Core.Resource.latest.json")]
    [InlineData("Core.Component.latest.json")]
    [InlineData("Core.Script.latest.json")]
    public void Common_cache_assets_are_present_and_non_empty(string fileName)
    {
        using var json = AcceptanceAssetLoader.LoadJson(fileName);

        json.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        json.RootElement.GetRawText().Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public void App_export_asset_is_present_and_contains_items()
    {
        using var json = AcceptanceAssetLoader.LoadJson("App.1.Export.json");

        json.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        json.RootElement.TryGetProperty("value", out var value).Should().BeTrue();
        value.ValueKind.Should().Be(JsonValueKind.Array);
        value.GetArrayLength().Should().BeGreaterThan(0);
        value[0].TryGetProperty("Items", out var items).Should().BeTrue();
        items.ValueKind.Should().Be(JsonValueKind.Array);
        items.GetArrayLength().Should().BeGreaterThan(0);
    }
}



