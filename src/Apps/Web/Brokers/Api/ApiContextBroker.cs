// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;

namespace Web.Brokers.Api;

internal sealed class ApiContextBroker(
    IServiceProvider serviceProvider)
    : IApiContextBroker
{
    public ApiInfo[] SelectAllApiInfos() =>
        [.. serviceProvider.GetServices<ApiInfo>()];

    public async ValueTask<string> ReadRequestBodyAsync(Stream requestBody)
    {
        using StreamReader reader = new(stream: requestBody);

        return await reader.ReadToEndAsync();
    }
}