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

public class ExcelFormatter : TextOutputFormatter
{
    public ExcelFormatter()
    {
        SupportedMediaTypes.Add(
item: MediaTypeHeaderValue.Parse(
input: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            )
        );
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/vnd.ms-excel"));
        SupportedEncodings.Add(item: Encoding.UTF8);
    }

    protected override bool CanWriteType(Type type) =>
        true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        string culture = GetCulture(context: context, selectedEncoding: selectedEncoding);
        await FormatterODataHelper
            .HandleOData(contextObject: context.Object)
            .ToExcel(resources: GetResources(context: context), culture: culture)
            .CopyToAsync(destination: context.HttpContext.Response.Body);
        await context.HttpContext.Response.Body.FlushAsync();
        context.HttpContext.Response.Body.Close();
    }

    private static string GetCulture(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (selectedEncoding == null)
        {
            throw new ArgumentNullException(nameof(selectedEncoding));
        }

        return context.HttpContext.Request.Query.ContainsKey(key: "culture")
            ? Thread.CurrentThread.CurrentCulture.Name
            : context.HttpContext.Request.Query["culture"].ToString();
    }

    public override void WriteResponseHeaders(OutputFormatterWriteContext context)
    {
        base.WriteResponseHeaders(context: context);
        context.HttpContext.Response.Headers["Content-Disposition"] =
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