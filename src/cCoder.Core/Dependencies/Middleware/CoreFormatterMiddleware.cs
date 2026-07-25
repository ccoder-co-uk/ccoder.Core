// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Primitives;

namespace cCoder.Core.Dependencies.Middleware;

internal sealed class CoreFormatterMiddleware : IMiddleware
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        Dictionary<string, StringValues> query =
            Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                queryString: context.Request.QueryString.Value);

        if (query.ContainsKey(key: "t"))
        {
            context.Request.Headers["Authorization"] =
                $"bearer {query["t"][0]}";
        }

        if (query.TryGetValue(
            key: "$format",
            value: out StringValues value))
        {
            context.Request.Headers["Accept"] = value[0] switch
            {
                "xml" => "application/xml",
                "csv" => "text/csv",
                "excel" =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => context.Request.Headers["Content-Type"],
            };

            context.Response.Headers["Content-Disposition"] =
                query["$format"][0] switch
                {
                    "xml" => "attachment; filename=export.xml",
                    "csv" => "attachment; filename=export.csv",
                    "excel" => "attachment; filename=export.xlsx",
                    _ => "attachment; filename=export.json",
                };
        }

        await next(context: context);
    }
}