// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Brokers.Setup;

internal sealed class CoreSetupContextBroker(
    ICoreContextFactory coreContextFactory)
    : ICoreSetupContextBroker
{
    public DbContext CreateCoreContext() =>
        coreContextFactory.CreateCoreContext();
}