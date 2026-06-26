using System.IO.Compression;
using cCoder.Data.Models.CMS;


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
        new FormatterCsvFileBuilder
        {
            Resources = resources ?? [],
            Culture = culture,
            Delimiter = delimiter,
            Quotes = quotes,
        }.BuildFor(source);

    public static Stream ToExcel(
        this object source,
        IEnumerable<Resource> resources,
        string culture = ""
    ) => new FormatterExcelFileBuilder(culture, resources ?? []).BuildFor(source);

    public static Resource ForNameAndCulture(
        this IEnumerable<Resource> potentials,
        string name,
        string culture
    )
    {
        List<Resource> results = [];

        foreach (
            IEnumerable<Resource> resourceGroup in potentials
                .Where(resource =>
                    string.Equals(
                        resource.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .GroupBy(resource => resource.Name, StringComparer.OrdinalIgnoreCase)
        )
        {
            Resource resource = resourceGroup.GetClosestCulturalMatch(culture);

            if (resource != null)
            {
                results.Add(resource);
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
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        int take = cultureParts.Count;
        string resultCulture = string.Empty;

        while (result == null && resultCulture != null)
        {
            resultCulture = string.Join("-", cultureParts.Take(take));
            result = potentials?.FirstOrDefault(resource =>
                string.Equals(
                    resource.Culture,
                    resultCulture,
                    StringComparison.OrdinalIgnoreCase
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
            result = potentials?.FirstOrDefault(resource =>
                string.IsNullOrEmpty(resource.Culture)
            );
        }

        return result;
    }

    public static void AddTextFile(this ZipArchive zip, string path, string text)
    {
        ZipArchiveEntry entry = zip.CreateEntry(path, CompressionLevel.Optimal);

        using Stream stream = entry.Open();
        using StreamWriter writer = new(stream);
        writer.Write(text);
    }
}

