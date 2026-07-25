// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Orchestrations;

internal sealed partial class HostedServicesAppSecurityAppAddOrchestrationService
{
    private static void ValidateAppOnHandle(App app) =>
        ValidationRulesEngine.Validate(inputs: [app]);
}