// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.Net.Http.Headers;


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

        JsonNode json = JsonSerializer.SerializeToNode(
            value: new { item = HandleOData(contextObject: context.Object) });

        XElement root = new(name: "root");
        AppendJsonNode(parent: root, node: json);
        XDocument xml = new(content: root);

        return new StringBuilder(xml.ToString());
    }

    private static void AppendJsonNode(XElement parent, JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach ((string name, JsonNode value) in jsonObject)
            {
                if (value is JsonArray jsonArray)
                {
                    foreach (JsonNode item in jsonArray)
                    {
                        XElement child = new(name: name);
                        AppendJsonNode(parent: child, node: item);
                        parent.Add(content: child);
                    }

                    continue;
                }

                XElement property = new(name: name);
                AppendJsonNode(parent: property, node: value);
                parent.Add(content: property);
            }

            return;
        }

        if (node is JsonArray array)
        {
            foreach (JsonNode item in array)
            {
                XElement child = new(name: "item");
                AppendJsonNode(parent: child, node: item);
                parent.Add(content: child);
            }

            return;
        }

        if (node is JsonValue jsonValue)
        {
            parent.Value = jsonValue.TryGetValue(out string stringValue)
                ? stringValue
                : jsonValue.ToJsonString();
        }
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