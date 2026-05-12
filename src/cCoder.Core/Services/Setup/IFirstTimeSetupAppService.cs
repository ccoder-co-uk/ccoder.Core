using cCoder.Data.Models.CMS;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Setup;

internal interface IFirstTimeSetupAppService
{
    Task<App> CreateFirstAppAsync(
        FirstTimeSetupRequest request,
        string tenantId,
        CancellationToken cancellationToken = default);
}
