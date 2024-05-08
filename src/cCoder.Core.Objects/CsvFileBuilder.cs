using cCoder.Core.Objects.Entities.CMS;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace cCoder.Core.Objects;

public class CsvFileBuilder
{
    public IEnumerable<Resource> Resources { get; set; } = Array.Empty<Resource>();
    public string Delimiter { get; set; } = ";";
    public string Quotes { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;

    public string BuildFor(object o)
    {
        string dateFormat = Resources.FirstOrDefault(r => r.Name == "dateformat")?.DisplayName ?? "yyyy-MM-ddThh:mm:ssZ";
        string moneyFormat = Resources.FirstOrDefault(r => r.Name == "moneyformat")?.DisplayName ?? "n";

        if (o is IEnumerable)
        {
            object[] arr = (o as IEnumerable<object>).ToArray();

            if (arr.Any())
            {
                PropertyInfo[] props = arr[0].GetType().GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).ToArray();

                string header = arr[0] is IDictionary<string, object> dictionary
                    ? string.Join(Delimiter, dictionary.Keys.Select(k => $"{Quotes}{k}{Quotes}").ToArray()) + "\n"
                    : string.Join(Delimiter, props.Select(p => Resources.FirstOrDefault(r => r.Name == p.Name)?.ShortDisplayName ?? p.Name)) + "\n";

                return BuildFinalOutput(dateFormat, moneyFormat, arr, props, header);
            }
            else
                return string.Empty;
        }
        else
        {
            IEnumerable<PropertyInfo> props = o.GetType().GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));
            string header = string.Join(Delimiter, props.Select(p => $"{Quotes}{p.Name}{Quotes}"));
            return header + "\n" + string.Join(Delimiter, props.Select(p => FormatCsvValue(p.GetValue(o), dateFormat, moneyFormat)));
        }
    }

    private string BuildFinalOutput(string dateFormat, string moneyFormat, object[] arr, PropertyInfo[] props, string header)
    {
        StringBuilder builder = new(header);

        foreach (object i in arr)
            _ = builder.Append(BuildObjectCSVString(i, props, dateFormat, moneyFormat));

        return builder.ToString();
    }

    private string BuildObjectCSVString(object source, PropertyInfo[] props, string dateFormat, string moneyFormat)
    {
        if (source is IDictionary<string, object> dict)
        {
            string[] keys = dict.Keys.ToArray();
            string[] values = new string[keys.Length];

            for (int x = 0; x < keys.Length; x++)
                values[x] = FormatCsvValue(dict[keys[x]], dateFormat, moneyFormat);

            return $"{string.Join(Delimiter, values)}\n";
        }
        else
        {
            string[] values = new string[props.Length];

            for (int x = 0; x < props.Length; x++)
                values[x] = FormatCsvValue(props[x].GetValue(source), dateFormat, moneyFormat);

            return $"{string.Join(Delimiter, values)}\n";
        }
    }

    private string FormatCsvValue(object v, string dateFormat, string moneyFormat) => v switch
    {
        DateTime dt => $"{Quotes}{dt.ToString(dateFormat, CultureInfo.CreateSpecificCulture(Culture))}{Quotes}",
        DateTimeOffset dto => dto.ToString(dateFormat, CultureInfo.CreateSpecificCulture(Culture)),
        decimal dto => dto.ToString(moneyFormat, CultureInfo.CreateSpecificCulture(Culture)),
        Guid g => $"{Quotes}{g}{Quotes}",
        null => string.Empty,
        _ => $"{Quotes}{v}{Quotes}",
    };
}
