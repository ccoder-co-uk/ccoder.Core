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
        TypedRow[] input = CreateTypedRows();

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
            .Contain(expected: "<t>Date</t>");

        sheet.Should()
            .Contain(expected: "<t>Amount</t>");

        sheet.Should()
            .Contain(expected: "<t>Count</t>");

        sheet.Should()
            .Contain(expected: "<t>IntegerValue</t>");

        sheet.Should()
            .Contain(expected: "<t>DoubleAmount</t>");

        sheet.Should()
            .Contain(expected: "<t>FloatAmount</t>");

        sheet.Should()
            .Contain(expected: "<t>IsPaid</t>");

        sheet.Should()
            .Contain(expected: "<t>Identifier</t>");

        sheet.Should()
            .Contain(expected: "<t>Character</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>OccurredOnArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>DateArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>AmountArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>IntegerArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>DoubleArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>FloatArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>PaidArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>IdentifierArray</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>Tags</t>");

        sheet.Should()
            .NotContain(unexpected: "<t>Characters</t>");

        sheet.Should()
            .Contain(expected: "<t>Alpha</t>");

        sheet.Should()
            .Contain(expected: "<t>Beta</t>");
    }

    [Fact]
    public void ShouldIncludeScalarPropertiesInTypedObjectCsvHeadersAndRows()
    {
        // Given
        TypedRow[] input = CreateTypedRows();

        CsvFileProcessingService service = new(
            resources: [],
            delimiter: ",",
            quotes: "",
            culture: "en-GB");

        // When
        string csv = service.BuildCsvFile(source: input);

        string header = ReadCsvHeader(csv: csv);

        // Then
        header.Should()
            .Contain(expected: "Name");

        header.Should()
            .Contain(expected: "OccurredOn");

        header.Should()
            .Contain(expected: "Date");

        header.Should()
            .Contain(expected: "Amount");

        header.Should()
            .Contain(expected: "Count");

        header.Should()
            .Contain(expected: "IntegerValue");

        header.Should()
            .Contain(expected: "DoubleAmount");

        header.Should()
            .Contain(expected: "FloatAmount");

        header.Should()
            .Contain(expected: "IsPaid");

        header.Should()
            .Contain(expected: "Identifier");

        header.Should()
            .Contain(expected: "Character");

        header.Should()
            .NotContain(unexpected: "OccurredOnArray");

        header.Should()
            .NotContain(unexpected: "DateArray");

        header.Should()
            .NotContain(unexpected: "AmountArray");

        header.Should()
            .NotContain(unexpected: "IntegerArray");

        header.Should()
            .NotContain(unexpected: "DoubleArray");

        header.Should()
            .NotContain(unexpected: "FloatArray");

        header.Should()
            .NotContain(unexpected: "PaidArray");

        header.Should()
            .NotContain(unexpected: "IdentifierArray");

        header.Should()
            .NotContain(unexpected: "Tags");

        header.Should()
            .NotContain(unexpected: "Characters");

        csv.Should()
            .Contain(expected: "Alpha");

        csv.Should()
            .Contain(expected: "Beta");
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

    [Fact]
    public void ShouldIncludeStringValuesInDictionaryCsvHeadersAndRows()
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

        CsvFileProcessingService service = new(
            resources: [],
            delimiter: ",",
            quotes: "",
            culture: "en-GB");

        // When
        string csv = service.BuildCsvFile(source: input);

        string header = ReadCsvHeader(csv: csv);

        // Then
        header.Should()
            .Contain(expected: "Name");

        header.Should()
            .Contain(expected: "Amount");

        header.Should()
            .NotContain(unexpected: "Tags");

        csv.Should()
            .Contain(expected: "Alpha");
    }

    private static TypedRow[] CreateTypedRows() =>
        [
        new(
            Name: "Alpha",
            OccurredOn: DateTimeOffset.Parse(input: "2026-09-11T12:00:00+00:00"),
            Date: new(
                year: 2026,
                month: 9,
                day: 11,
                hour: 12,
                minute: 0,
                second: 0,
                kind: DateTimeKind.Utc),
            Amount: 12.5m,
            Count: 1,
            IntegerValue: 42,
            DoubleAmount: 12.75d,
            FloatAmount: 6.25f,
            IsPaid: true,
            Identifier: Guid.Parse(input: "d738c780-1b8e-4286-97dd-30698faeaf21"),
            Character: 'A',
            Tags: ["one"],
            Characters: ['A', 'B'],
            OccurredOnArray: [DateTimeOffset.Parse(input: "2026-09-11T12:00:00+00:00")],
            DateArray: [new DateTime(year: 2026, month: 9, day: 11)],
            AmountArray: [12.5m],
            IntegerArray: [42],
            DoubleArray: [12.75d],
            FloatArray: [6.25f],
            PaidArray: [true],
            IdentifierArray: [Guid.Parse(input: "d738c780-1b8e-4286-97dd-30698faeaf21")]),
        new(
            Name: "Beta",
            OccurredOn: DateTimeOffset.Parse(input: "2026-09-11T13:00:00+00:00"),
            Date: new(
                year: 2026,
                month: 9,
                day: 11,
                hour: 13,
                minute: 0,
                second: 0,
                kind: DateTimeKind.Utc),
            Amount: 24.5m,
            Count: null,
            IntegerValue: 84,
            DoubleAmount: 24.75d,
            FloatAmount: 12.5f,
            IsPaid: false,
            Identifier: Guid.Parse(input: "4bb7c5a7-7434-46cb-af3a-5c829e7f2af0"),
            Character: 'B',
            Tags: ["two"],
            Characters: ['C', 'D'],
            OccurredOnArray: [DateTimeOffset.Parse(input: "2026-09-11T13:00:00+00:00")],
            DateArray: [new DateTime(year: 2026, month: 9, day: 11)],
            AmountArray: [24.5m],
            IntegerArray: [84],
            DoubleArray: [24.75d],
            FloatArray: [12.5f],
            PaidArray: [false],
            IdentifierArray: [Guid.Parse(input: "4bb7c5a7-7434-46cb-af3a-5c829e7f2af0")]),
    ];

    private static string ReadCsvHeader(string csv) =>
        csv[..csv.IndexOf(value: '\n')];

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
        DateTime Date,
        decimal Amount,
        int? Count,
        int IntegerValue,
        double DoubleAmount,
        float FloatAmount,
        bool IsPaid,
        Guid Identifier,
        char Character,
        string[] Tags,
        char[] Characters,
        DateTimeOffset[] OccurredOnArray,
        DateTime[] DateArray,
        decimal[] AmountArray,
        int[] IntegerArray,
        double[] DoubleArray,
        float[] FloatArray,
        bool[] PaidArray,
        Guid[] IdentifierArray);
}