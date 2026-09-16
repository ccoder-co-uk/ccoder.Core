// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;
using Web.Services.Foundations.Api;

namespace Web.Exposures;

internal sealed class ApiContextManager(
    IApiContextService apiContextService)
    : IApiContextManager
{
    public ApiInfo[] GetApiInfos() =>
        apiContextService.GetApiInfos();

    public ValueTask<string> ReadRequestBodyAsync(Stream requestBody) =>
        apiContextService.ReadRequestBodyAsync(requestBody: requestBody);
}