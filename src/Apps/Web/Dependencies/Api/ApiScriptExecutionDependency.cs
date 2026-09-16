// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text;
using cCoder.Core.Models;

namespace Web.Dependencies.Api;

internal sealed class ApiScriptExecutionDependency
    : HttpClient
{
    public ApiScriptExecutionDependency(CoreConfiguration configuration)
        : base(new HttpClientHandler
        {
            AutomaticDecompression =
                DecompressionMethods.GZip
                | DecompressionMethods.Deflate
        })
    {
        BaseAddress = new Uri(
            uriString: configuration.Workflow.ServiceUrl);
        Timeout = TimeSpan.FromMinutes(value: 10);
    }

    public async ValueTask<string> ExecuteScriptAsync(string script)
    {
        using StringContent content = new(
            content: script,
            encoding: Encoding.UTF8,
            mediaType: "text/plain");

        using HttpResponseMessage response =
            await PostAsync(
                requestUri: "ExecuteScript",
                content: content);

        return await response.Content.ReadAsStringAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}