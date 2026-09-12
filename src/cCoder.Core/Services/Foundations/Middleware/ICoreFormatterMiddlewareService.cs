// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Middleware;

using cCoder.Core.Models.Middleware;

internal interface ICoreFormatterMiddlewareService
{
    IReadOnlyDictionary<string, string> ParseQuery(string queryString);
    Task InvokeCoreFormatterMiddlewareInvocationNextAsync(
        CoreFormatterMiddlewareInvocation coreFormatterMiddlewareInvocation);
}