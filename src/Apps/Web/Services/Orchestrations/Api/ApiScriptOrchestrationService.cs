// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
using Web.Services.Foundations.Api;

namespace Web.Services.Orchestrations.Api;

internal sealed partial class ApiScriptOrchestrationService(
    IApiScriptAuthorizationService apiScriptAuthorizationService,
    IApiScriptExecutionService apiScriptExecutionService)
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
}