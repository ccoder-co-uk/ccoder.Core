// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using CoreApp = cCoder.Data.Models.CMS.App;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class TemplatedEmailOrchestrationService
{
    private static void ValidateAppTemplatedEmailOnQueue(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName) =>
        ValidationRulesEngine.Validate(
            inputs:
            [
                app,
                templateName,
                culture,
                model,
                toEmail,
                subject,
                sentByUserId,
                mailSenderName
            ]);

    private static void ValidateTemplatedEmailDetailsOnQueue(
        TemplatedEmailDetails details) =>
        ValidationRulesEngine.Validate(inputs: [details]);
}