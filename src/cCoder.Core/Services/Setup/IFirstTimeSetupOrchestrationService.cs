using cCoder.Core.Models;

namespace cCoder.Core.Services.Setup;

public interface IFirstTimeSetupOrchestrationService
{
    Task<FirstTimeSetupResult> SetupAsync(
        FirstTimeSetupRequest request,
        CancellationToken cancellationToken = default);
}
