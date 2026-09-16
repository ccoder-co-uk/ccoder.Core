// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Text;
using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.Net.Http.Headers;


namespace cCoder.Core.Dependencies.Formatters;

public sealed partial class CsvFormatter : TextOutputFormatter
{
    private readonly IEnumerable<Resource> resources;
    private readonly string delimiter;
    private readonly string quotes;
    private readonly string culture;

    public CsvFormatter()
        : this(
            resources: [],
            delimiter: ", ",
            quotes: "",
            culture: "en-GB")
    {
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "application/csv"));
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/csv"));
        SupportedEncodings.Add(item: Encoding.UTF8);
    }

    internal CsvFormatter(
        IEnumerable<Resource> resources,
        string delimiter,
        string quotes,
        string culture)
    {
        this.resources = resources ?? [];
        this.delimiter = delimiter;
        this.quotes = quotes;
        this.culture = culture;
    }

    protected override bool CanWriteType(Type type) =>
        true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        (string delimiter, string quotes, string culture) = ExtractValues(context: context);

        await context.HttpContext.Response.WriteAsync(
text: new CsvFormatter(
                    resources: GetResources(context: context, culture: culture),
                    delimiter: delimiter,
                    quotes: quotes,
                    culture: culture)
                .BuildCsvFile(source: HandleOData(contextObject: context.Object))
        );
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

    private static (string delimiter, string quotes, string culture) ExtractValues(
        OutputFormatterWriteContext context
    )
    {
        ArgumentNullException.ThrowIfNull(argument: context);

        return (
            context.HttpContext.Request.Query.ContainsKey(key: "delimiter")
                ? context.HttpContext.Request.Query["delimiter"].ToString()
                : ", ",
            context.HttpContext.Request.Query.ContainsKey(key: "quotes")
                ? context.HttpContext.Request.Query["quotes"].ToString()
                : "",
            context.HttpContext.Request.Query.ContainsKey(key: "culture")
                ? context.HttpContext.Request.Query["culture"].ToString()
                : "en-GB"
        );
    }

    private static IEnumerable<Resource> GetResources(
        OutputFormatterWriteContext context,
        string culture
    )
    {
        var commonObjectCache = context.HttpContext.RequestServices.GetRequiredService<ICommonObjectCache>();
        Resource[] cachedResources = commonObjectCache.GetAll<Resource>();
        List<Resource> resources = [];

        if (context.HttpContext.Request.Query.ContainsKey(key: "appId"))
        {
            resources.AddRange(
collection: cachedResources
                    .Where(predicate: r =>
                        r.AppId == int.Parse(s: context.HttpContext.Request.Query["appId"].ToString())
                        && r.Key == "Default"
                        && r.Culture == culture
                    )
            );
        }

        resources.AddRange(
collection:
            [
                new()
                {
                    Name = "dateformat",
                    DisplayName = context.HttpContext.Request.Query.ContainsKey(key: "dateFormat")
                        ? context.HttpContext.Request.Query["dateFormat"].ToString()
                        : "yyyy-MM-dd",
                },
                new()
                {
                    Name = "moneyformat",
                    DisplayName = context.HttpContext.Request.Query.ContainsKey(key: "moneyFormat")
                        ? context.HttpContext.Request.Query["moneyFormat"].ToString()
                        : "n",
                },
            ]
        );

        resources.AddRange(collection: cachedResources);
        return [.. resources];
    }
}