// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Middleware;

internal sealed partial class CoreFormatterMiddlewareProcessingService
{
    private static void ValidateOnProcess(
        HttpContext context,
        RequestDelegate next) =>
        ValidationRulesEngine.Validate(inputs: [context, next]);
}