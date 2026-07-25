// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text;

namespace Web.Brokers.Api;

internal sealed class ApiScriptExecutionBroker(
    IConfiguration configuration)
    : IApiScriptExecutionBroker
{
    public async ValueTask<string> ExecuteScriptAsync(
        string script)
    {
        using HttpClient api = new(
            handler: new HttpClientHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.GZip
                    | DecompressionMethods.Deflate
            })
        {
            BaseAddress = new Uri(
                uriString: configuration[
                    "Services:Workflow"]!),
            Timeout = TimeSpan.FromMinutes(
                value: 10)
        };

        using StringContent content = new(
            content: script,
            encoding: Encoding.UTF8,
            mediaType: "text/plain");

        using HttpResponseMessage response =
            await api.PostAsync(
                requestUri: "ExecuteScript",
                content: content);

        return await response.Content
            .ReadAsStringAsync();
    }
}