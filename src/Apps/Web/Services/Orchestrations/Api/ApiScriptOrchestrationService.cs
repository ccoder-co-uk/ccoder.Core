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
        ApiScriptRequest request) =>
        TryCatch(operation: async () =>
        {
            ValidateApiScriptRequestOnExecute(
                request: request);

            apiScriptAuthorizationService
                .AuthorizeScriptExecution();

            return await apiScriptExecutionService
                .ExecuteScriptAsync(
                    script: request.Script);
        });
}