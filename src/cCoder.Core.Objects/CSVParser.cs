using System.Dynamic;
using System.Reflection;

namespace cCoder.Core.Objects;

public static class CSVParser<T> where T : new()
{
    public static IEnumerable<T> Parse(string csvData, CSVParseConfig options)
    {
        List<T> result = new();
        options ??= CSVParseConfig.DefaultOptions;
        using (StringReader r = new(csvData))
        {
            options.FieldNames = GetFieldNames(r, options);
            string line = r.ReadLine();
            while (line != null)
            {
                result.Add(ParseLine(line, options));
                line = r.ReadLine();
            }
        }

        return result;
    }

    private static string[] GetFieldNames(StringReader csvReader, CSVParseConfig options) => options.FieldNamesInHeader
            ? csvReader.ReadLine().Split(options.Separator)
            : options.FieldNames ?? Enumerable.Range(0, 50).Select(i => $"Value{i}").ToArray();

    // ParseFromCsv<dynamic> is a bit of an exceptional case and needs to be handled differently
    private static T ParseLine(string csvLine, CSVParseConfig options)
        => typeof(object) == typeof(T)
            ? ParseDynamicData(csvLine, options)
            : ParseDataOfTypeT(csvLine, options);

    private static T ParseDataOfTypeT(string csvLine, CSVParseConfig options)
    {
        T result = new();
        PropertyInfo[] props = typeof(T).GetProperties();

        string[] dataItems = csvLine.Split(options.Separator);

        if (!options.FieldNamesInHeader)
        {
            int offset = 0;
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo p = props[i];

                if (p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                    p.SetValue(result, dataItems[i - offset]);
            }
        }
        else
            for (int i = 0; i < dataItems.Length; i++)
                SetDataItem(options, result, props, dataItems, i);

        return result;
    }

    private static void SetDataItem(CSVParseConfig options, T result, PropertyInfo[] props, string[] dataItems, int i)
    {
        string field = options.FieldNames[i];
        string value = dataItems[i];
        PropertyInfo prop = props.FirstOrDefault(p => p.Name.ToLowerInvariant() == field.ToLowerInvariant());

        if (prop != null && !string.IsNullOrEmpty(value))
        {
            try
            {
                if (prop.PropertyType == typeof(double))
                    prop.SetValue(result, double.Parse(value));
                else if (prop.PropertyType == typeof(decimal))
                    prop.SetValue(result, decimal.Parse(value));
                else if (prop.PropertyType == typeof(bool?) || prop.PropertyType == typeof(bool))
                    prop.SetValue(result, bool.Parse(value));
                else
                    prop.SetValue(result, value);
            }
            catch (Exception)
            {
                prop.SetValue(result, null);
            }
        }
    }

    private static dynamic ParseDynamicData(string csvLine, CSVParseConfig options)
    {
        dynamic result = new ExpandoObject();
        string[] dataItems = csvLine.Split(options.Separator);

        if (!options.FieldNamesInHeader)
        {
            string[] fn = options.FieldNames;
            for (int i = 0; i < dataItems.Length; i++)
            {
                string field = (fn != null && fn.Length > i) ? fn[i].Replace(" ", "_") : "Item" + (i + 1);
                ((IDictionary<string, object>)result).Add(field, dataItems[i]);
            }
        }
        else
        {
            for (int i = 0; i < options.FieldNames.Length; i++)
            {
                try
                {
                    ((IDictionary<string, object>)result).Add(options.FieldNames[i], dataItems[i]);
                }
                catch (Exception)
                {
                    ((IDictionary<string, object>)result).Add(options.FieldNames[i], null);
                }
            }
        }

        return result;
    }
}

public class CSVParseConfig
{
    public bool FieldNamesInHeader { get; set; }
    public char Separator { get; set; }
    public string[] FieldNames { get; set; }

    public static readonly CSVParseConfig DefaultOptions = new()
    {
        FieldNamesInHeader = false,
        Separator = ','
    };
}
