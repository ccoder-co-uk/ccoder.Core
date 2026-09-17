// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
using System.IO;
using Web.Exposures;
using Web.Services.Foundations.Api;

namespace Web.Services.Orchestrations.Api;

internal sealed partial class ApiScriptOrchestrationService(
    IApiScriptAuthorizationService apiScriptAuthorizationService,
    IApiScriptExecutionService apiScriptExecutionService,
    IApiContextService apiContextService)
    : IApiScriptOrchestrationService
{
    public ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest apiScriptRequest) =>
        TryCatch(operation: async () =>
        {
            ValidateApiScriptRequestOnExecute(
                apiScriptRequest: apiScriptRequest);

            apiScriptAuthorizationService
                .AuthorizeScriptExecution();

            return await apiScriptExecutionService
                .ExecuteScriptAsync(
                    script: apiScriptRequest.Script);
        });

    public ValueTask<string> ReadRequestBodyAsync(Stream requestBody) =>
        TryCatch(operation: async () =>
        {
            ValidateRequestBodyOnRead(requestBody: requestBody);

            return await apiScriptExecutionService.ReadRequestBodyAsync(
                requestBody: requestBody);
        });

    void IApiScriptManager.LogError(Exception exception) =>
        apiContextService.LogError(exception: exception);
}