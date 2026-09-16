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

public sealed partial class ExcelFormatter : TextOutputFormatter
{
    private readonly string culture;
    private readonly IEnumerable<Resource> resources;

    public ExcelFormatter()
        : this(culture: "", resources: [])
    {
        SupportedMediaTypes.Add(
item: MediaTypeHeaderValue.Parse(
input: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            )
        );
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/vnd.ms-excel"));
        SupportedEncodings.Add(item: Encoding.UTF8);
    }

    internal ExcelFormatter(
        string culture,
        IEnumerable<Resource> resources)
    {
        this.culture = culture;
        this.resources = resources ?? [];
    }

    protected override bool CanWriteType(Type type) =>
        true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        string culture = GetCulture(context: context, selectedEncoding: selectedEncoding);

        byte[] workbook = new ExcelFormatter(
                culture: culture,
                resources: GetResources(context: context))
            .BuildExcelFile(data: HandleOData(contextObject: context.Object))
            ;

        await context.HttpContext.Response.Body.WriteAsync(
            buffer: workbook);

        await context.HttpContext.Response.Body.FlushAsync();
        context.HttpContext.Response.Body.Close();
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

    private static string GetCulture(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        ArgumentNullException.ThrowIfNull(argument: selectedEncoding);

        return context.HttpContext.Request.Query.ContainsKey(key: "culture")
            ? Thread.CurrentThread.CurrentCulture.Name
            : context.HttpContext.Request.Query["culture"].ToString();
    }

    public override void WriteResponseHeaders(OutputFormatterWriteContext context)
    {
        base.WriteResponseHeaders(context: context);

        context.HttpContext.Response.Headers.ContentDisposition =
            "Content-Disposition: attachment; Data.xlsx;";
    }

    private static IEnumerable<Resource> GetResources(
        OutputFormatterWriteContext context
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