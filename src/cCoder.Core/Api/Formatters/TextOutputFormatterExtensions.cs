using System.Collections;
using System.Dynamic;
using Microsoft.AspNetCore.OData.Query.Wrapper;


namespace cCoder.Core.Api.Formatters;

public static class FormatterODataHelper
{
    public static object HandleOData(object contextObject)
    {
        if (contextObject is IEnumerable enumerable and not string)
        {
            return ProcessIEumerable(enumerable);
        }
        else
        {
            object result = UnpackSelectExpandWrapper(contextObject);
            if (result is IDictionary<string, object> dict)
            {
                ProcessDictionary(dict);
            }

            return result;
        }
    }

    private static dynamic[] ProcessIEumerable(IEnumerable enumerable)
    {
        dynamic[] rawDataItems = enumerable
            .Cast<object>()
            .Select(i => UnpackSelectExpandWrapper(i))
            .ToArray();

        foreach (dynamic item in rawDataItems)
        {
            if (item is IDictionary<string, object> dict)
            {
                ProcessDictionary(dict);
            }
        }

        return rawDataItems;
    }

    private static object UnpackSelectExpandWrapper(object contextObject) =>
        (contextObject is ISelectExpandWrapper wrapper)
            ? ToExpandoObject(wrapper.ToDictionary())
            : contextObject;

    private static ExpandoObject ToExpandoObject(IDictionary<string, object> source)
    {
        ExpandoObject result = new();
        IDictionary<string, object> resultDictionary = result;

        foreach ((string key, object value) in source)
        {
            resultDictionary[key] = value;
        }

        return result;
    }

    private static void ProcessDictionary(IDictionary<string, object> dict)
    {
        string[] keys = dict.Keys.ToArray();
        foreach (string key in keys)
        {
            dict[key] = HandleOData(dict[key]);
        }
    }
}




