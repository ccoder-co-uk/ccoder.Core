using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupStateService(
    ICoreContextFactory coreContextFactory,
    ISecurityDbContextFactory securityDbContextFactory) : IFirstTimeSetupStateService
{
    public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
    {
        await using DbContext core = coreContextFactory.CreateCoreContext();
        await using DbContext sso = securityDbContextFactory.CreateDbContext(true);

        bool hasApp = await core.Set<App>()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken);

        if (!hasApp)
            return false;

        return await sso.Set<Tenant>()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken);
    }
}
