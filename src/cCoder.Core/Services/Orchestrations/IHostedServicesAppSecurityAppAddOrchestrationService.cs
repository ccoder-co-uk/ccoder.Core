// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Orchestrations;

internal interface IHostedServicesAppSecurityAppAddOrchestrationService
{
    ValueTask HandleAppAsync(App app);
}