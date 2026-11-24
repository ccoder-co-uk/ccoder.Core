// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;

using cCoder.Security.Models.Entities;

namespace cCoder.Core.Services.Aggregations.Security;

internal interface ITenantAdministrationAggregationService
{
    ValueTask<Tenant> AddTenantAsync(Tenant newTenant);

    ValueTask DeleteTenantAsync(Tenant deletedTenant);

    IQueryable<Tenant> GetAllTenants();

    ValueTask<Tenant> UpdateTenantAsync(Tenant updatedTenant);
}