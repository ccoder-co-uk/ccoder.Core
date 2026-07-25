// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class CsvFileProcessingService(
    IEnumerable<Resource> resources,
    string delimiter,
    string quotes,
    string culture)
    : ICsvFileProcessingService
{
    private readonly IEnumerable<Resource> resources = resources ?? [];
    private readonly string delimiter = delimiter;
    private readonly string quotes = quotes;
    private readonly string culture = culture;

    public string BuildCsvFile(object source) =>
        TryCatch(operation: () =>
        {
            ValidateCsvFileOnBuild(source: source);

            string dateFormat = resources
                .FirstOrDefault(
                    predicate: resource => resource.Name == "dateformat")
                ?.DisplayName
                ?? "yyyy-MM-ddThh:mm:ssZ";

            string moneyFormat = resources
                .FirstOrDefault(
                    predicate: resource => resource.Name == "moneyformat")
                ?.DisplayName
                ?? "n";

            if (source is IEnumerable enumerable)
            {
                object[] items = [.. enumerable.Cast<object>()];

                if (items.Length == 0)
                {
                    return string.Empty;
                }

                PropertyInfo[] properties = SelectProperties(
                    source: items[0]);

                string header = BuildHeader(
                    source: items[0],
                    properties: properties);

                return BuildFinalOutput(
                    dateFormat: dateFormat,
                    moneyFormat: moneyFormat,
                    items: items,
                    properties: properties,
                    header: header);
            }

            PropertyInfo[] sourceProperties = SelectProperties(
                source: source);

            string sourceHeader = string.Join(
                separator: delimiter,
                values: sourceProperties.Select(
                    selector: property =>
                        $"{quotes}{property.Name}{quotes}"));

            string sourceValues = string.Join(
                separator: delimiter,
                values: sourceProperties.Select(
                    selector: property => FormatCsvValue(
                        value: property.GetValue(obj: source),
                        dateFormat: dateFormat,
                        moneyFormat: moneyFormat)));

            return $"{sourceHeader}\n{sourceValues}";
        });

    private static PropertyInfo[] SelectProperties(object source) =>
        [.. source.GetType()
            .GetProperties()
            .Where(
                predicate: property =>
                    property.PropertyType.IsValueType
                    || property.PropertyType == typeof(string))];

    private string BuildHeader(
        object source,
        PropertyInfo[] properties)
    {
        IEnumerable<string> headings =
            source is IDictionary<string, object> dictionary
                ? dictionary.Keys.Select(
                    selector: key => $"{quotes}{key}{quotes}")
                : properties.Select(
                    selector: property => resources
                        .FirstOrDefault(
                            predicate: resource =>
                                resource.Name == property.Name)
                        ?.ShortDisplayName
                        ?? property.Name);

        return $"{string.Join(separator: delimiter, values: headings)}\n";
    }

    private string BuildFinalOutput(
        string dateFormat,
        string moneyFormat,
        object[] items,
        PropertyInfo[] properties,
        string header)
    {
        StringBuilder builder = new(value: header);

        foreach (object item in items)
        {
            _ = builder.Append(
                value: BuildObjectCsvString(
                    source: item,
                    properties: properties,
                    dateFormat: dateFormat,
                    moneyFormat: moneyFormat));
        }

        return builder.ToString();
    }

    private string BuildObjectCsvString(
        object source,
        PropertyInfo[] properties,
        string dateFormat,
        string moneyFormat)
    {
        if (source is IDictionary<string, object> dictionary)
        {
            string[] values = [.. dictionary.Values
                .Select(
                    selector: value => FormatCsvValue(
                        value: value,
                        dateFormat: dateFormat,
                        moneyFormat: moneyFormat))];

            return $"{string.Join(separator: delimiter, value: values)}\n";
        }

        string[] propertyValues = [.. properties
            .Select(
                selector: property => FormatCsvValue(
                    value: property.GetValue(obj: source),
                    dateFormat: dateFormat,
                    moneyFormat: moneyFormat))];

        return string.Join(
            separator: delimiter,
            value: propertyValues)
            + "\n";
    }

    private string FormatCsvValue(
        object value,
        string dateFormat,
        string moneyFormat) =>
        value switch
        {
            DateTime dateTime =>
                $"{quotes}{dateTime.ToString(
                    format: dateFormat,
                    provider: CultureInfo.CreateSpecificCulture(
                        name: culture))}{quotes}",
            DateTimeOffset dateTimeOffset =>
                dateTimeOffset.ToString(
                    format: dateFormat,
                    formatProvider: CultureInfo.CreateSpecificCulture(
                        name: culture)),
            decimal decimalValue =>
                decimalValue.ToString(
                    format: moneyFormat,
                    provider: CultureInfo.CreateSpecificCulture(
                        name: culture)),
            Guid guid => $"{quotes}{guid}{quotes}",
            null => string.Empty,
            _ => $"{quotes}{value}{quotes}",
        };
}