// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
namespace cCoder.Core.Brokers.Middleware;

internal interface ICoreFormatterMiddlewareBroker
{
    IReadOnlyDictionary<string, string> ParseQuery(string queryString);
    Task InvokeNextAsync(object next, object context);
}