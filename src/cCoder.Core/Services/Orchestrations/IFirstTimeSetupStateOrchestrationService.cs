// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
namespace cCoder.Core.Services.Orchestrations;

public interface IFirstTimeSetupStateOrchestrationService
{
    Task<bool> IsInitializedAsync(
        CancellationToken cancellationToken = default);

    string NormalizeHost(string host);
}