// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text;
using cCoder.Core.Models;

namespace Web.Brokers.Api;

internal sealed class ApiScriptExecutionBroker(
    CoreConfiguration configuration)
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
                uriString: configuration.Workflow.ServiceUrl),
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