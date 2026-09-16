// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Dynamic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;


namespace cCoder.Core.Dependencies.Formatters;

public class XmlFormatter : TextOutputFormatter
{
    public XmlFormatter()
    {
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "application/xml"));
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/xml"));

        SupportedEncodings.Add(item: Encoding.UTF8);
        SupportedEncodings.Add(item: Encoding.Unicode);
    }

    protected override bool CanWriteType(Type type) =>
        true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        StringBuilder buffer = GetBuffer(context: context, selectedEncoding: selectedEncoding);
        await context.HttpContext.Response.WriteAsync(text: buffer.ToString());
    }

    private StringBuilder GetBuffer(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        ArgumentNullException.ThrowIfNull(argument: selectedEncoding);

        string json = JsonConvert.SerializeObject(
value: new { item = HandleOData(contextObject: context.Object) }
        );

        System.Xml.Linq.XDocument xml = JsonConvert.DeserializeXNode(value: json, deserializeRootElementName: "root");
        return new StringBuilder(xml.ToString());
    }

    private static object HandleOData(object contextObject)
    {
        if (contextObject is IEnumerable enumerable and not string)
        {
            return ProcessEnumerable(enumerable: enumerable);
        }

        object result = UnpackSelectExpandWrapper(contextObject: contextObject);

        if (result is IDictionary<string, object> dictionary)
        {
            ProcessDictionary(dictionary: dictionary);
        }

        return result;
    }

    private static dynamic[] ProcessEnumerable(IEnumerable enumerable)
    {
        dynamic[] rawDataItems = [.. enumerable
            .Cast<object>()
            .Select(selector: item => UnpackSelectExpandWrapper(
                contextObject: item))];

        foreach (dynamic item in rawDataItems)
        {
            if (item is IDictionary<string, object> dictionary)
            {
                ProcessDictionary(dictionary: dictionary);
            }
        }

        return rawDataItems;
    }

    private static object UnpackSelectExpandWrapper(object contextObject)
    {
        object unpacked = contextObject is ISelectExpandWrapper wrapper
            ? wrapper.ToDictionary()
            : contextObject;

        return unpacked is IDictionary<string, object> dictionary
            ? ToExpandoObject(source: dictionary)
            : unpacked;
    }

    private static ExpandoObject ToExpandoObject(
        IDictionary<string, object> source)
    {
        ExpandoObject result = new();
        IDictionary<string, object> resultDictionary = result;

        foreach ((string key, object value) in source)
        {
            resultDictionary[key] = value;
        }

        return result;
    }

    private static void ProcessDictionary(
        IDictionary<string, object> dictionary)
    {
        string[] keys = [.. dictionary.Keys];

        foreach (string key in keys)
        {
            dictionary[key] = HandleOData(contextObject: dictionary[key]);
        }
    }
}