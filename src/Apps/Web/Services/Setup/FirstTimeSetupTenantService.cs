using cCoder.Security.Data.Models;
using cCoder.Security.Exposures;
using cCoder.Security.Objects.Entities;
using Web.Models;

namespace Web.Services.Setup;

internal sealed class FirstTimeSetupTenantService(
    ITenantManager tenantManager)
    : IFirstTimeSetupTenantService
{
    public async Task<string> SetupSecurityAsync(
        FirstTimeSetupRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        string tenantName = request.TenantName.Trim();
        string tenantId = FirstTimeSetupIdentifiers.BuildTenantId(tenantName);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await tenantManager.SetupAsync(
            new SetupDetails
            {
                Tenant = new Tenant
                {
                    Id = tenantId,
                    Name = tenantName,
                    Description = $"{tenantName} tenant",
                    CreatedBy = userId,
                    LastUpdatedBy = userId,
                    CreatedOn = now,
                    LastUpdated = now
                },
                User = new SSOUser
                {
                    Id = userId,
                    DisplayName = request.DisplayName.Trim(),
                    Email = request.Email.Trim(),
                    PasswordHash = request.Password
                }
            });

        return tenantId;
    }
}

