// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Services.Foundations.Api;

internal interface IApiScriptExecutionService
{
    ValueTask<string> ExecuteScriptAsync(
        string script);
}