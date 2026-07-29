// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        using JsonDocument document = JsonDocument.Parse(json: result);

        string[] typeNames = [.. document.RootElement
            .EnumerateArray()
            .SelectMany(selector: typeSet => typeSet.GetProperty(propertyName: "Types")
            .EnumerateArray())
            .Select(selector: type => type.GetProperty(propertyName: "Name")
            .GetString())
            .Where(predicate: typeName => !string.IsNullOrWhiteSpace(value: typeName))
            .Cast<string>()
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)];

        typeNames.Should()
            .Contain(
expected:         [
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

        string[] contextTypes = [.. document.RootElement
            .EnumerateArray()
            .SelectMany(selector: typeSet =>
            {
                string contextName = typeSet.GetProperty(propertyName: "Name")
                    .GetString() ?? string.Empty;

                return typeSet.GetProperty(propertyName: "Types")
                    .EnumerateArray()
                    .Select(selector: type => $"{contextName}/{type.GetProperty(propertyName: "Name")
                    .GetString()}");
            })];

        contextTypes.Should()
            .Contain(
expected:         [
            "Workflow/FlowDefinition",
        ]);

        contextTypes.Should()
            .NotContain(
unexpected:         [
            "Core/BusinessProcess",
            "Workflow/BusinessProcess",
        ]);
    }
}