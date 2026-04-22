using cCoder.Data.Models.CMS;
using Web.Models;

namespace Web.Services.Setup;

internal interface IFirstTimeSetupAppService
{
    Task<App> CreateFirstAppAsync(
        FirstTimeSetupRequest request,
        string tenantId,
        CancellationToken cancellationToken = default);
}
