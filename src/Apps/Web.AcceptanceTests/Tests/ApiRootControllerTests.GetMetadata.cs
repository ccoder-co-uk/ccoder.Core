using System.Text.Json;
using FluentAssertions;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class ApiRootControllerTests
{
    [Fact]
    public async Task ShouldReturnAggregateMetadataForGetMetadata()
    {
        // Given

        // When
        string result = await GetMetadataAsync();

        // Then
        using JsonDocument document = JsonDocument.Parse(result);

        string[] typeNames = document.RootElement
            .EnumerateArray()
            .SelectMany(typeSet => typeSet.GetProperty("Types").EnumerateArray())
            .Select(type => type.GetProperty("Name").GetString())
            .Where(typeName => !string.IsNullOrWhiteSpace(typeName))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        typeNames.Should().Contain(
        [
            "Role",
            "Privilege",
            "User",
            "UserRole",
            "File",
            "Folder",
            "FolderRole",
            "MailServer",
            "QueuedEmail",
            "SentEmail",
            "Calendar",
            "CalendarEvent",
            "ScheduledTask",
            "Package",
            "PackageItem",
            "FlowDefinition",
            "FlowInstanceData",
            "WorkflowEvent",
            "LogEntry",
            "LogDataItem",
        ]);
    }
}



