// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO.Compression;
using cCoder.Data.Models.CMS;
using cCoder.Core.Services.Processings.Formatters;


namespace cCoder.Core.Exposures.Formatters;

internal static class FormatterExportExtensions
{
    public static string ToCsv(
        this object source,
        IEnumerable<Resource> resources,
        string delimiter = ";",
        string quotes = "",
        string culture = ""
    ) =>
        new CsvFileProcessingService(
            resources: resources,
            delimiter: delimiter,
            quotes: quotes,
            culture: culture)
            .BuildCsvFile(source: source);

    public static Stream ToExcel(
        this object source,
        IEnumerable<Resource> resources,
        string culture = ""
    ) =>
        new ExcelFileProcessingService(
            culture: culture,
            resources: resources)
            .BuildExcelFile(data: source);

    public static Resource ForNameAndCulture(
        this IEnumerable<Resource> potentials,
        string name,
        string culture
    )
    {
        List<Resource> results = [];

        foreach (
            IEnumerable<Resource> resourceGroup in potentials
                .Where(predicate: resource =>
                    string.Equals(
a: resource.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase
                    )
                )
                .GroupBy(keySelector: resource => resource.Name, comparer: StringComparer.OrdinalIgnoreCase)
        )
        {
            Resource resource = resourceGroup.GetClosestCulturalMatch(culture: culture);

            if (resource != null)
            {
                results.Add(item: resource);
            }
        }

        return results.FirstOrDefault();
    }

    public static Resource GetClosestCulturalMatch(
        this IEnumerable<Resource> potentials,
        string culture
    )
    {
        Resource result = null;

        List<string> cultureParts = (culture ?? string.Empty)
            .ToLowerInvariant()
            .Split(separator: '-', options: StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        int take = cultureParts.Count;
        string resultCulture = string.Empty;

        while (result == null && resultCulture != null)
        {
            resultCulture = string.Join(separator: "-", values: cultureParts.Take(count: take));

            result = potentials?.FirstOrDefault(predicate: resource =>
                string.Equals(
a: resource.Culture, b: resultCulture, comparisonType: StringComparison.OrdinalIgnoreCase
                )
            );

            take--;

            if (take == 0)
            {
                resultCulture = null;
            }
        }

        if (result == null)
        {
            result = potentials?.FirstOrDefault(predicate: resource =>
                string.IsNullOrEmpty(value: resource.Culture)
            );
        }

        return result;
    }

    public static void AddTextFile(this ZipArchive zip, string path, string text)
    {
        ZipArchiveEntry entry = zip.CreateEntry(entryName: path, compressionLevel: CompressionLevel.Optimal);

        using Stream stream = entry.Open();
        using StreamWriter writer = new(stream);
        writer.Write(value: text);
    }
}