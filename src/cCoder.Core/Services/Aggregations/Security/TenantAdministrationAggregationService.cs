// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;

using cCoder.ContentManagement.Exposures;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Exceptions;
using cCoder.Security.Services.Aggregations.Interfaces;

namespace cCoder.Core.Services.Aggregations.Security;

internal sealed partial class TenantAdministrationAggregationService(
    ITenantAggregationService tenantAggregationService,
    IAppManager appManager)
        : ITenantAdministrationAggregationService
{
    public ValueTask<Tenant> AddTenantAsync(Tenant newTenant) =>
        TryCatch(operation: async () =>
        {
            ValidateTenantOnAdd(newTenant: newTenant);

            return await tenantAggregationService.AddTenantAsync(item: newTenant);
        });

    public ValueTask DeleteTenantAsync(Tenant deletedTenant)
        =>
        TryCatch(operation: async () =>
        {
            ValidateTenantOnDelete(deletedTenant: deletedTenant);

            if (appManager
                .GetAll(ignoreFilters: true)
                .Any(predicate: app => app.TenantId == deletedTenant.Id))
            {
                throw new SecurityAggregationValidationException(
                    innerException: new ArgumentException(
                        message: $"Tenant '{deletedTenant.Id}' cannot be deleted while applications are attached.",
                        paramName: nameof(deletedTenant)));
            }

            await tenantAggregationService.DeleteTenantAsync(item: deletedTenant);
        });

    public IQueryable<Tenant> GetAllTenants() =>
        TryCatch(operation: () =>
        {
            return tenantAggregationService.GetAllTenants();
        });

    public ValueTask<Tenant> UpdateTenantAsync(Tenant updatedTenant) =>
        TryCatch(operation: async () =>
        {
            ValidateTenantOnUpdate(updatedTenant: updatedTenant);

            return await tenantAggregationService.UpdateTenantAsync(item: updatedTenant);
        });
}