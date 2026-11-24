// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;

using cCoder.CodeAnalysis.Exposures;
using cCoder.Core.Services.Aggregations.Security;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Entities;

namespace cCoder.Core.Exposures;

internal sealed class CoreTenantAdministrationManager(
    ITenantAdministrationAggregationService tenantAdministrationAggregationService)
        : ITenantAdministrationManager, ICompositionExposure
{
    public ValueTask<Tenant> AddTenantAsync(Tenant newTenant) =>
        tenantAdministrationAggregationService.AddTenantAsync(
            newTenant: newTenant);

    public ValueTask DeleteTenantAsync(Tenant deletedTenant) =>
        tenantAdministrationAggregationService.DeleteTenantAsync(
            deletedTenant: deletedTenant);

    public IQueryable<Tenant> GetAllTenants() =>
        tenantAdministrationAggregationService.GetAllTenants();

    public ValueTask<Tenant> UpdateTenantAsync(Tenant updatedTenant) =>
        tenantAdministrationAggregationService.UpdateTenantAsync(
            updatedTenant: updatedTenant);
}