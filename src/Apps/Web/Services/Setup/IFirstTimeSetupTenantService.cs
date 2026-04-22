using Web.Models;

namespace Web.Services.Setup;

internal interface IFirstTimeSetupTenantService
{
    Task<string> SetupSecurityAsync(
        FirstTimeSetupRequest request,
        string userId,
        CancellationToken cancellationToken = default);
}
