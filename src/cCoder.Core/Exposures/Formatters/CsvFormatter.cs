// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq.Dynamic.Core;
using System.Text;
using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;


namespace cCoder.Core.Exposures.Formatters;

public class CsvFormatter : TextOutputFormatter
{
    public CsvFormatter()
    {
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "application/csv"));
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/csv"));
        SupportedEncodings.Add(item: Encoding.UTF8);
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
text: FormatterODataHelper
                .HandleOData(contextObject: context.Object)
                .ToCsv(resources: GetResources(context: context, culture: culture), delimiter: delimiter, quotes: quotes, culture: culture)
        );
    }

    private static (string delimiter, string quotes, string culture) ExtractValues(
        OutputFormatterWriteContext context
    )
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

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
collection: new Resource[]
            {
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
            }
        );
        resources.AddRange(collection: cachedResources);
        return resources.ToArray();
    }
}