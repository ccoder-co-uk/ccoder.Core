// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.IO;

namespace Web.Services.Foundations.Api;

internal interface IApiScriptExecutionService
{
    ValueTask<string> ExecuteScriptAsync(
        string script);

    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
}