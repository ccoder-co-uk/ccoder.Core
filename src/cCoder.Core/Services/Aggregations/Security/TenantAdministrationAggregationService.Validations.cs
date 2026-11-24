// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Security.Models.Entities;

namespace cCoder.Core.Services.Aggregations.Security;

internal sealed partial class TenantAdministrationAggregationService
{
    private static void Validate(params object[] inputs) =>
        ValidationRulesEngine.Validate(inputs: inputs);

    private static void ValidateTenantOnAdd(Tenant newTenant) =>
        Validate(inputs: [newTenant]);

    private static void ValidateTenantOnDelete(Tenant deletedTenant) =>
        Validate(inputs: [deletedTenant]);

    private static void ValidateTenantOnUpdate(Tenant updatedTenant) =>
        Validate(inputs: [updatedTenant]);
}