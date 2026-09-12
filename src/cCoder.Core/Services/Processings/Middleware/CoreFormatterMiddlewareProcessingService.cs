// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Middleware;
using cCoder.Core.Models.Middleware;

namespace cCoder.Core.Services.Processings.Middleware;

internal sealed partial class CoreFormatterMiddlewareProcessingService(
    ICoreFormatterMiddlewareService coreFormatterMiddlewareService)
    : ICoreFormatterMiddlewareProcessingService
{
    public Task ProcessAsync(
        HttpContext context,
        RequestDelegate next) =>
        TryCatch(operation: async () =>
        {
            ValidateOnProcess(context: context, next: next);

            IReadOnlyDictionary<string, string> query =
                coreFormatterMiddlewareService.ParseQuery(
                    queryString: context.Request.QueryString.Value);

            if (query.TryGetValue(
                key: "t",
                value: out string token))
            {
                context.Request.Headers.Authorization =
                    $"bearer {token}";
            }

            if (query.TryGetValue(
                key: "$format",
                value: out string value))
            {
                context.Request.Headers.Accept = value switch
                {
                    "xml" => "application/xml",
                    "csv" => "text/csv",
                    "excel" =>
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => context.Request.Headers.ContentType,
                };

                context.Response.Headers.ContentDisposition =
                    value switch
                    {
                        "xml" => "attachment; filename=export.xml",
                        "csv" => "attachment; filename=export.csv",
                        "excel" => "attachment; filename=export.xlsx",
                        _ => "attachment; filename=export.json",
                    };
            }

            await coreFormatterMiddlewareService
                .InvokeCoreFormatterMiddlewareInvocationNextAsync(
                coreFormatterMiddlewareInvocation: new CoreFormatterMiddlewareInvocation
                {
                    Next = next,
                    Context = context
                });
        });
}