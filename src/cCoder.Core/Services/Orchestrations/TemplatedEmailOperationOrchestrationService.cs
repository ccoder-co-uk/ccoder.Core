// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.TemplatedEmails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOperationOrchestrationService(
    ITemplatedEmailContentService templatedEmailContentService,
    ITemplatedEmailIdentityService templatedEmailIdentityService,
    ITemplatedEmailQueueService templatedEmailQueueService
) : ITemplatedEmailOperationOrchestrationService
{
    public ValueTask<TemplatedEmailOperation> QueueTemplatedEmailOperationAsync(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateTemplatedEmailOperationOnQueue(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailContentService.ResolveTemplatedEmailOperationContent(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailIdentityService.ResolveTemplatedEmailOperationIdentity(
                templatedEmailOperation: templatedEmailOperation);

            templatedEmailContentService.RenderTemplatedEmailOperationContent(
                templatedEmailOperation: templatedEmailOperation);

            return await templatedEmailQueueService
                .QueueTemplatedEmailOperationAsync(
                    templatedEmailOperation: templatedEmailOperation);
        });
}