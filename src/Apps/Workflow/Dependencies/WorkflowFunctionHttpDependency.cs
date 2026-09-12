// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text;

namespace Workflow.Dependencies;

internal sealed class WorkflowFunctionHttpDependency
{
    public async ValueTask<string> ReadBodyAsync(HttpRequestData request)
    {
        using WorkflowFunctionStreamDependency content = new();
        await request.Body.CopyToAsync(destination: content);

        return Encoding.UTF8.GetString(bytes: content.ToArray());
    }

    public async Task<HttpResponseData> CreateResponseAsync(
        HttpRequestData request,
        string content)
    {
        HttpResponseData response = request.CreateResponse(
            statusCode: HttpStatusCode.OK);

        await response.WriteStringAsync(value: content);

        return response;
    }
}