// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Core;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Brokers.Setup;

internal sealed class CoreSetupContextBroker(
    ICoreContextFactory coreContextFactory)
    : ICoreSetupContextBroker
{
    public async ValueTask<bool> IsInitializedAsync(
        CancellationToken cancellationToken)
    {
        await using DbContext context = coreContextFactory.CreateCoreContext();

        return await SetupDatabase.IsInitializedAsync<App>(
            context: context,
            cancellationToken: cancellationToken);
    }
}