// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Entities;
using cCoder.Core;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Brokers.Setup;

internal sealed class SecuritySetupContextBroker(
    ISecurityDbContextFactory securityDbContextFactory)
    : ISecuritySetupContextBroker
{
    public async ValueTask<bool> IsInitializedAsync(
        CancellationToken cancellationToken)
    {
        await using DbContext context = securityDbContextFactory.CreateDbContext(
            ignoreAuthInfo: true);

        return await SetupDatabase.IsInitializedAsync<Tenant>(
            context: context,
            cancellationToken: cancellationToken);
    }
}