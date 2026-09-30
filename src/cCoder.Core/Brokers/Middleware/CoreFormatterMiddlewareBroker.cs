// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

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