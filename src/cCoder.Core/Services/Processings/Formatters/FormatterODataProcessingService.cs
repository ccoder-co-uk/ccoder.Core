// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Dynamic;
using cCoder.Core.Services.Foundations.Formatters;


namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class FormatterODataProcessingService(
    IFormatterODataService formatterODataService)
    : IFormatterODataProcessingService
{
    public object HandleOData(object contextObject) =>
        TryCatch(operation: () =>
        {
            ValidateContextObjectOnHandle(contextObject: contextObject);

            return HandleODataObject(contextObject: contextObject);
        });

    private object HandleODataObject(object contextObject)
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

    private dynamic[] ProcessIEumerable(IEnumerable enumerable)
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

    private object UnpackSelectExpandWrapper(object contextObject)
    {
        object unpacked = formatterODataService.UnpackSelectExpandWrapper(
            contextObject: contextObject);

        return unpacked is IDictionary<string, object> dictionary
            ? ToExpandoObject(source: dictionary)
            : unpacked;
    }

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

    private void ProcessDictionary(IDictionary<string, object> dict)
    {
        string[] keys = [.. dict.Keys];

        foreach (string key in keys)
        {
            dict[key] = HandleODataObject(contextObject: dict[key]);
        }
    }
}