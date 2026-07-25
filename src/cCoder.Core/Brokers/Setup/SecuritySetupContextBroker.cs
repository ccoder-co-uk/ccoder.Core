// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Data.EF.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Brokers.Setup;

internal sealed class SecuritySetupContextBroker(
    ISecurityDbContextFactory securityDbContextFactory)
    : ISecuritySetupContextBroker
{
    public DbContext CreateSecurityContext() =>
        securityDbContextFactory.CreateDbContext(
            ignoreAuthInfo: true);
}