// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace cCoder.Core.Brokers.Middleware;

internal sealed class CoreFormatterMiddlewareBroker
    : ICoreFormatterMiddlewareBroker
{
    public IReadOnlyDictionary<string, string> ParseQuery(string queryString) =>
        QueryHelpers.ParseQuery(queryString: queryString)
            .ToDictionary(
                keySelector: item => item.Key,
                elementSelector: item => item.Value.FirstOrDefault());

    public Task InvokeNextAsync(object next, object context) =>
        ((RequestDelegate)next).Invoke(context: (HttpContext)context);
}