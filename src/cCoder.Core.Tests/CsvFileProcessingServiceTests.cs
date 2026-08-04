// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Formatters;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CsvFileProcessingServiceTests
{
    [Fact]
    public void ShouldBuildCsvForSingleObject()
    {
        // Given
        Guid identifier = Guid.Parse(input: "d738c780-1b8e-4286-97dd-30698faeaf21");

        object input = new
        {
            Name = "Alpha",
            Identifier = identifier,
        };

        CsvFileProcessingService service = new(
            resources: [],
            delimiter: ",",
            quotes: "\"",
            culture: "en-GB");

        // When
        string actualCsv = service.BuildCsvFile(source: input);

        // Then
        actualCsv
            .Should()
            .Be(expected: $"\"Name\",\"Identifier\"\n\"Alpha\",\"{identifier}\"");
    }

    [Fact]
    public void ShouldBuildCsvForObjectCollection()
    {
        // Given
        object[] input =
        [
            new { Name = "Alpha", Count = 1 },
            new { Name = "Beta", Count = 2 },
        ];

        CsvFileProcessingService service = new(
            resources: [],
            delimiter: ";",
            quotes: "'",
            culture: "en-GB");

        // When
        string actualCsv = service.BuildCsvFile(source: input);

        // Then
        actualCsv
            .Should()
            .Be(expected: "Name;Count\n'Alpha';'1'\n'Beta';'2'\n");
    }

    [Fact]
    public void ShouldReturnEmptyCsvForEmptyCollection()
    {
        // Given
        CsvFileProcessingService service = new(
            resources: [],
            delimiter: ",",
            quotes: "\"",
            culture: "en-GB");

        // When
        string actualCsv = service.BuildCsvFile(source: Array.Empty<object>());

        // Then
        actualCsv
            .Should()
            .BeEmpty();
    }
}