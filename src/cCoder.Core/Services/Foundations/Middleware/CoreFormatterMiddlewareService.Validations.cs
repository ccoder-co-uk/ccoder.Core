// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models.Middleware;

namespace cCoder.Core.Services.Foundations.Middleware;

internal sealed partial class CoreFormatterMiddlewareService
{
    private static void ValidateQueryStringOnParse(string queryString) =>
        ValidationRulesEngine.Validate(inputs: [queryString]);

    private static void ValidateCoreFormatterMiddlewareInvocationOnInvokeNext(
        CoreFormatterMiddlewareInvocation coreFormatterMiddlewareInvocation) =>
        ValidationRulesEngine.Validate(inputs: [coreFormatterMiddlewareInvocation]);
}