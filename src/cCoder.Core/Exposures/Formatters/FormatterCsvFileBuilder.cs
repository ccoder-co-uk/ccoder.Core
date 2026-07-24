// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using cCoder.Data.Models.CMS;


namespace cCoder.Core.Exposures.Formatters;

internal class FormatterCsvFileBuilder
{
    public IEnumerable<Resource> Resources { get; set; } = [];
    public string Delimiter { get; set; } = ";";
    public string Quotes { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;

    public string BuildFor(object source)
    {
        string dateFormat =
            Resources.FirstOrDefault(predicate: resource => resource.Name == "dateformat")?.DisplayName
            ?? "yyyy-MM-ddThh:mm:ssZ";

        string moneyFormat =
            Resources.FirstOrDefault(predicate: resource => resource.Name == "moneyformat")?.DisplayName
            ?? "n";

        if (source is IEnumerable enumerable)
        {
            object[] items = enumerable.Cast<object>()
                .ToArray();

            if (!items.Any())
            {
                return string.Empty;
            }

            PropertyInfo[] properties = items[0]
                .GetType()
                .GetProperties()
                .Where(predicate: property =>
                    property.PropertyType.IsValueType || property.PropertyType == typeof(string)
                )
                .ToArray();

            string header = items[0] is IDictionary<string, object> dictionary
                ? string.Join(
separator: Delimiter, values: dictionary.Keys.Select(selector: key => $"{Quotes}{key}{Quotes}")
                    )
                    + "\n"
                : string.Join(
separator: Delimiter, values: properties.Select(selector: property =>
                            Resources.FirstOrDefault(predicate: resource => resource.Name == property.Name)
                                ?.ShortDisplayName ?? property.Name
                        )
                    )
                    + "\n";

            return BuildFinalOutput(dateFormat: dateFormat, moneyFormat: moneyFormat, items: items, properties: properties, header: header);
        }

        IEnumerable<PropertyInfo> sourceProperties = source
            .GetType()
            .GetProperties()
            .Where(predicate: property =>
                property.PropertyType.IsValueType || property.PropertyType == typeof(string)
            );

        string sourceHeader = string.Join(
separator: Delimiter, values: sourceProperties.Select(selector: property => $"{Quotes}{property.Name}{Quotes}")
        );

        return sourceHeader
            + "\n"
            + string.Join(
separator: Delimiter, values: sourceProperties.Select(selector: property =>
                    FormatCsvValue(value: property.GetValue(obj: source), dateFormat: dateFormat, moneyFormat: moneyFormat)
                )
            );
    }

    private string BuildFinalOutput(
        string dateFormat,
        string moneyFormat,
        object[] items,
        PropertyInfo[] properties,
        string header
    )
    {
        StringBuilder builder = new(header);

        foreach (object item in items)
        {
            _ = builder.Append(
value: BuildObjectCsvString(source: item, properties: properties, dateFormat: dateFormat, moneyFormat: moneyFormat)
            );
        }

        return builder.ToString();
    }

    private string BuildObjectCsvString(
        object source,
        PropertyInfo[] properties,
        string dateFormat,
        string moneyFormat
    )
    {
        if (source is IDictionary<string, object> dictionary)
        {
            string[] keys = dictionary.Keys.ToArray();
            string[] values = new string[keys.Length];

            for (int index = 0; index < keys.Length; index++)
            {
                values[index] = FormatCsvValue(
value: dictionary[keys[index]], dateFormat: dateFormat, moneyFormat: moneyFormat
                );
            }

            return $"{string.Join(separator: Delimiter, value: values)}\n";
        }

        string[] propertyValues = new string[properties.Length];

        for (int index = 0; index < properties.Length; index++)
        {
            propertyValues[index] = FormatCsvValue(
value: properties[index].GetValue(obj: source), dateFormat: dateFormat, moneyFormat: moneyFormat
            );
        }

        return $"{string.Join(separator: Delimiter, value: propertyValues)}\n";
    }

    private string FormatCsvValue(object value, string dateFormat, string moneyFormat) =>
        value switch
        {
            DateTime dateTime =>
                $"{Quotes}{dateTime.ToString(format: dateFormat, provider: CultureInfo.CreateSpecificCulture(name: Culture))}{Quotes}",
            DateTimeOffset dateTimeOffset =>
                dateTimeOffset.ToString(
format: dateFormat, formatProvider: CultureInfo.CreateSpecificCulture(name: Culture)
                ),
            decimal decimalValue =>
                decimalValue.ToString(format: moneyFormat, provider: CultureInfo.CreateSpecificCulture(name: Culture)),
            Guid guid => $"{Quotes}{guid}{Quotes}",
            null => string.Empty,
            _ => $"{Quotes}{value}{Quotes}",
        };
}