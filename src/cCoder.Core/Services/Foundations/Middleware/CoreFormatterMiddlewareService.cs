// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Middleware;
using cCoder.Core.Models.Middleware;

namespace cCoder.Core.Services.Foundations.Middleware;

internal sealed partial class CoreFormatterMiddlewareService(
    ICoreFormatterMiddlewareBroker coreFormatterMiddlewareBroker)
    : ICoreFormatterMiddlewareService
{
    public IReadOnlyDictionary<string, string> ParseQuery(string queryString) =>
        TryCatch(operation: () =>
        {
            ValidateQueryStringOnParse(queryString: queryString);

            return coreFormatterMiddlewareBroker.ParseQuery(
                queryString: queryString);
        });

    public Task InvokeCoreFormatterMiddlewareInvocationNextAsync(
        CoreFormatterMiddlewareInvocation coreFormatterMiddlewareInvocation) =>
        TryCatch(operation: async () =>
        {
            ValidateCoreFormatterMiddlewareInvocationOnInvokeNext(
                coreFormatterMiddlewareInvocation: coreFormatterMiddlewareInvocation);

            await coreFormatterMiddlewareBroker.InvokeNextAsync(
                next: coreFormatterMiddlewareInvocation.Next,
                context: coreFormatterMiddlewareInvocation.Context);
        });
}