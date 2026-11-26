// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using cCoder.Core.Models;
using Microsoft.Extensions.Configuration;

namespace Web.Dependencies.Api;

internal sealed class ApiScriptExecutionDependency
    : HttpClient
{
    private readonly IConfiguration runtimeConfiguration;

    public ApiScriptExecutionDependency(
        CoreConfiguration configuration,
        IConfiguration runtimeConfiguration)
        : base(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression =
                DecompressionMethods.GZip
                | DecompressionMethods.Deflate
        })
    {
        this.runtimeConfiguration = runtimeConfiguration;
        BaseAddress = new Uri(
            uriString: configuration.Workflow.ServiceUrl);
        Timeout = TimeSpan.FromMinutes(value: 10);
    }

    public async ValueTask<string> ExecuteScriptAsync(string script)
    {
        string executionToken = runtimeConfiguration["Workflow:ExecutionToken"];
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: executionToken);

        using StringContent content = new(
            content: script,
            encoding: Encoding.UTF8,
            mediaType: "text/plain");

        using HttpRequestMessage request = new(method: HttpMethod.Post, requestUri: "ExecuteScript")
        {
            Content = content
        };

        request.Headers.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: executionToken);
        using HttpResponseMessage response = await SendAsync(request: request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}