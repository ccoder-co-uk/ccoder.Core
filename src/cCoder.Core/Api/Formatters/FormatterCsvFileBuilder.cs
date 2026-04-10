using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using cCoder.Data.Models.CMS;


namespace cCoder.Core.Api.Formatters;

internal class FormatterCsvFileBuilder
{
    public IEnumerable<Resource> Resources { get; set; } = [];
    public string Delimiter { get; set; } = ";";
    public string Quotes { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;

    public string BuildFor(object source)
    {
        string dateFormat =
            Resources.FirstOrDefault(resource => resource.Name == "dateformat")?.DisplayName
            ?? "yyyy-MM-ddThh:mm:ssZ";
        string moneyFormat =
            Resources.FirstOrDefault(resource => resource.Name == "moneyformat")?.DisplayName
            ?? "n";

        if (source is IEnumerable enumerable)
        {
            object[] items = enumerable.Cast<object>().ToArray();

            if (!items.Any())
            {
                return string.Empty;
            }

            PropertyInfo[] properties = items[0]
                .GetType()
                .GetProperties()
                .Where(property =>
                    property.PropertyType.IsValueType || property.PropertyType == typeof(string)
                )
                .ToArray();

            string header = items[0] is IDictionary<string, object> dictionary
                ? string.Join(
                        Delimiter,
                        dictionary.Keys.Select(key => $"{Quotes}{key}{Quotes}")
                    )
                    + "\n"
                : string.Join(
                        Delimiter,
                        properties.Select(property =>
                            Resources.FirstOrDefault(resource => resource.Name == property.Name)
                                ?.ShortDisplayName ?? property.Name
                        )
                    )
                    + "\n";

            return BuildFinalOutput(dateFormat, moneyFormat, items, properties, header);
        }

        IEnumerable<PropertyInfo> sourceProperties = source
            .GetType()
            .GetProperties()
            .Where(property =>
                property.PropertyType.IsValueType || property.PropertyType == typeof(string)
            );
        string sourceHeader = string.Join(
            Delimiter,
            sourceProperties.Select(property => $"{Quotes}{property.Name}{Quotes}")
        );

        return sourceHeader
            + "\n"
            + string.Join(
                Delimiter,
                sourceProperties.Select(property =>
                    FormatCsvValue(property.GetValue(source), dateFormat, moneyFormat)
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
                BuildObjectCsvString(item, properties, dateFormat, moneyFormat)
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
                    dictionary[keys[index]],
                    dateFormat,
                    moneyFormat
                );
            }

            return $"{string.Join(Delimiter, values)}\n";
        }

        string[] propertyValues = new string[properties.Length];

        for (int index = 0; index < properties.Length; index++)
        {
            propertyValues[index] = FormatCsvValue(
                properties[index].GetValue(source),
                dateFormat,
                moneyFormat
            );
        }

        return $"{string.Join(Delimiter, propertyValues)}\n";
    }

    private string FormatCsvValue(object value, string dateFormat, string moneyFormat) =>
        value switch
        {
            DateTime dateTime =>
                $"{Quotes}{dateTime.ToString(dateFormat, CultureInfo.CreateSpecificCulture(Culture))}{Quotes}",
            DateTimeOffset dateTimeOffset =>
                dateTimeOffset.ToString(
                    dateFormat,
                    CultureInfo.CreateSpecificCulture(Culture)
                ),
            decimal decimalValue =>
                decimalValue.ToString(moneyFormat, CultureInfo.CreateSpecificCulture(Culture)),
            Guid guid => $"{Quotes}{guid}{Quotes}",
            null => string.Empty,
            _ => $"{Quotes}{value}{Quotes}",
        };
}

