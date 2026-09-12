// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Middleware;

internal sealed class CoreFormatterMiddlewareInvocation
{
    public object Context { get; set; }
    public object Next { get; set; }
}