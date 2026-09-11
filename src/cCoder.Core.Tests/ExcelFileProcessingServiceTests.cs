// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO.Compression;
using System.Text;
using cCoder.Core.Services.Processings.Formatters;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ExcelFileProcessingServiceTests
{
    [Fact]
    public void ShouldIncludeScalarPropertiesInTypedObjectHeadersAndRows()
    {
        // Given
        TypedRow[] input =
        [
            new(
                Name: "Alpha",
                OccurredOn: DateTimeOffset.Parse(input: "2026-09-11T12:00:00+00:00"),
                Amount: 12.5m,
                Count: 1,
                Identifier: Guid.Parse(input: "d738c780-1b8e-4286-97dd-30698faeaf21"),
                Tags: ["one"]),
            new(
                Name: "Beta",
                OccurredOn: DateTimeOffset.Parse(input: "2026-09-11T13:00:00+00:00"),
                Amount: 24.5m,
                Count: null,
                Identifier: Guid.Parse(input: "4bb7c5a7-7434-46cb-af3a-5c829e7f2af0"),
                Tags: ["two"]),
        ];

        ExcelFileProcessingService service = new(
            culture: "en-GB",
            resources: []);

        // When
        using Stream workbook = service.BuildExcelFile(data: input);

        string sheet = ReadSheet(workbook: workbook);

        // Then
        sheet.Should()
            .Contain(expected: "<t>Name</t>");

        sheet.Should()
            .Contain(expected: "<t>OccurredOn</t>");

        sheet.Should()
            .Contain(expected: "<t>Amount</t>");

        sheet.Should()
            .Contain(expected: "<t>Count</t>");

        sheet.Should()
            .Contain(expected: "<t>Identifier</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>Tags</t>");

        sheet.Should()
            .Contain(expected: "<t>Alpha</t>");

        sheet.Should()
            .Contain(expected: "<t>Beta</t>");
    }

    [Fact]
    public void ShouldIncludeStringValuesInDictionaryHeadersAndRows()
    {
        // Given
        Dictionary<string, object>[] input =
        [
            new()
            {
                ["Name"] = "Alpha",
                ["Amount"] = 12.5m,
                ["Tags"] = new[] { "one" },
            },
        ];

        ExcelFileProcessingService service = new(
            culture: "en-GB",
            resources: []);

        // When
        using Stream workbook = service.BuildExcelFile(data: input);

        string sheet = ReadSheet(workbook: workbook);

        // Then
        sheet.Should()
            .Contain(expected: "<t>Name</t>");

        sheet.Should()
            .Contain(expected: "<t>Amount</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>Tags</t>");

        sheet.Should()
            .Contain(expected: "<t>Alpha</t>");
    }

    private static string ReadSheet(Stream workbook)
    {
        using ZipArchive archive =
            new(
                stream: workbook,
                mode: ZipArchiveMode.Read);

        ZipArchiveEntry entry =
            archive.GetEntry(entryName: "xl/worksheets/sheet1.xml");

        using StreamReader reader =
            new(
                stream: entry.Open(),
                encoding: Encoding.UTF8);

        return reader.ReadToEnd();
    }

    private sealed record TypedRow(
        string Name,
        DateTimeOffset OccurredOn,
        decimal Amount,
        int? Count,
        Guid Identifier,
        string[] Tags);
}