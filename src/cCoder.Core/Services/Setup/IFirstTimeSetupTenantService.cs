using cCoder.Core.Models;

namespace cCoder.Core.Services.Setup;

internal interface IFirstTimeSetupTenantService
{
    Task<string> SetupSecurityAsync(
        FirstTimeSetupRequest request,
        string userId,
        CancellationToken cancellationToken = default);
}
