// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Dynamic;
using Microsoft.AspNetCore.OData.Query.Wrapper;


namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class FormatterODataProcessingService
    : IFormatterODataProcessingService
{
    public object HandleOData(object contextObject) =>
        TryCatch(operation: () =>
        {
            ValidateContextObjectOnHandle(contextObject: contextObject);

            return HandleODataObject(contextObject: contextObject);
        });

    private static object HandleODataObject(object contextObject)
    {
        if (contextObject is IEnumerable enumerable and not string)
        {
            return ProcessIEumerable(enumerable: enumerable);
        }
        else
        {
            object result = UnpackSelectExpandWrapper(contextObject: contextObject);

            if (result is IDictionary<string, object> dict)
            {
                ProcessDictionary(dict: dict);
            }

            return result;
        }
    }

    private static dynamic[] ProcessIEumerable(IEnumerable enumerable)
    {
        dynamic[] rawDataItems = [.. enumerable
            .Cast<object>()
            .Select(selector: i => UnpackSelectExpandWrapper(contextObject: i))];

        foreach (dynamic item in rawDataItems)
        {
            if (item is IDictionary<string, object> dict)
            {
                ProcessDictionary(dict: dict);
            }
        }

        return rawDataItems;
    }

    private static object UnpackSelectExpandWrapper(object contextObject) =>
        (contextObject is ISelectExpandWrapper wrapper)
            ? ToExpandoObject(source: wrapper.ToDictionary())
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
        string[] keys = [.. dict.Keys];

        foreach (string key in keys)
        {
            dict[key] = HandleODataObject(contextObject: dict[key]);
        }
    }
}