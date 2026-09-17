// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.TemplatedEmails;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOperationOrchestrationService(
    ITemplatedEmailContentService templatedEmailContentService,
    ITemplatedEmailIdentityService templatedEmailIdentityService,
    ITemplatedEmailQueueService templatedEmailQueueService
) : ITemplatedEmailOperationOrchestrationService,
    ITemplatedEmailOrchestrationService
{
    public ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails templatedEmailDetails) =>
        TryCatch(operation: async () =>
        {
            ValidateTemplatedEmailDetailsOnQueue(
                templatedEmailDetails: templatedEmailDetails);

            TemplatedEmailOperation templatedEmailOperation = new()
            {
                Details = templatedEmailDetails,
            };

            ValidateTemplatedEmailOperationOnQueue(
                templatedEmailOperation: templatedEmailOperation);

            TemplatedEmailOperation completedOperation =
                await QueueTemplatedEmailOperationInternalAsync(
                    templatedEmailOperation: templatedEmailOperation);

            return completedOperation.Email;
        });

    ValueTask<TemplatedEmailOperation> ITemplatedEmailOperationOrchestrationService
        .QueueTemplatedEmailOperationAsync(
            TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: async () =>
        {
            ValidateTemplatedEmailOperationOnQueue(
                templatedEmailOperation: templatedEmailOperation);

            return await QueueTemplatedEmailOperationInternalAsync(
                templatedEmailOperation: templatedEmailOperation);
        });

    private async ValueTask<TemplatedEmailOperation>
        QueueTemplatedEmailOperationInternalAsync(
            TemplatedEmailOperation templatedEmailOperation)
    {
        templatedEmailContentService.ResolveTemplatedEmailOperationContent(
            templatedEmailOperation: templatedEmailOperation);

        templatedEmailIdentityService.ResolveTemplatedEmailOperationIdentity(
            templatedEmailOperation: templatedEmailOperation);

        templatedEmailContentService.RenderTemplatedEmailOperationContent(
            templatedEmailOperation: templatedEmailOperation);

        return await templatedEmailQueueService
            .QueueTemplatedEmailOperationAsync(
                templatedEmailOperation: templatedEmailOperation);
    }
}