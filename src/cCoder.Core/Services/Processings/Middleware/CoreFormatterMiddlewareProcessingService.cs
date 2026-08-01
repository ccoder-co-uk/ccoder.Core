// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Primitives;

namespace cCoder.Core.Services.Processings.Middleware;

internal sealed partial class CoreFormatterMiddlewareProcessingService
    : ICoreFormatterMiddlewareProcessingService
{
    public Task ProcessAsync(
        HttpContext context,
        RequestDelegate next) =>
        TryCatch(operation: async () =>
        {
            ValidateOnProcess(context: context, next: next);

            Dictionary<string, StringValues> query =
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                    queryString: context.Request.QueryString.Value);

            if (query.TryGetValue(
                key: "t",
                value: out StringValues token))
            {
                context.Request.Headers.Authorization =
                    $"bearer {token[0]}";
            }

            if (query.TryGetValue(
                key: "$format",
                value: out StringValues value))
            {
                context.Request.Headers.Accept = value[0] switch
                {
                    "xml" => "application/xml",
                    "csv" => "text/csv",
                    "excel" =>
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => context.Request.Headers.ContentType,
                };

                context.Response.Headers.ContentDisposition =
                    value[0] switch
                    {
                        "xml" => "attachment; filename=export.xml",
                        "csv" => "attachment; filename=export.csv",
                        "excel" => "attachment; filename=export.xlsx",
                        _ => "attachment; filename=export.json",
                    };
            }

            await next(context: context);
        });
}