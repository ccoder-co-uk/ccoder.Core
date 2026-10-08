// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core;

internal static class SetupDatabase
{
    internal static async ValueTask<bool> IsInitializedAsync<TEntity>(
        DbContext context,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        return await context.Set<TEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken: cancellationToken);
    }
}
