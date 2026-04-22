using Web.Models;

namespace Web.Services.Setup;

public interface IFirstTimeSetupOrchestrationService
{
    Task<FirstTimeSetupResult> SetupAsync(
        FirstTimeSetupRequest request,
        CancellationToken cancellationToken = default);
}
