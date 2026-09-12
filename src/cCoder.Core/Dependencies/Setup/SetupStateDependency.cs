// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Dependencies.Setup;

internal static class SetupStateDependency
{
    internal static async ValueTask<bool> IsCoreInitializedAsync(
        ICoreContextFactory coreContextFactory,
        CancellationToken cancellationToken)
    {
        await using DbContext context = coreContextFactory.CreateCoreContext();

        return await IsDatabaseInitializedAsync<App>(
            context: context,
            cancellationToken: cancellationToken);
    }

    internal static async ValueTask<bool> IsSecurityInitializedAsync(
        ISecurityDbContextFactory securityDbContextFactory,
        CancellationToken cancellationToken)
    {
        await using DbContext context = securityDbContextFactory.CreateDbContext(
            ignoreAuthInfo: true);

        return await IsDatabaseInitializedAsync<Tenant>(
            context: context,
            cancellationToken: cancellationToken);
    }

    private static async ValueTask<bool> IsDatabaseInitializedAsync<TEntity>(
        DbContext context,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (!await DatabaseExistsAsync(
            context: context,
            cancellationToken: cancellationToken))
        {
            return false;
        }

        return await context.Set<TEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken: cancellationToken);
    }

    private static async ValueTask<bool> DatabaseExistsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        string connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            return false;
        }

        SqlConnectionStringBuilder builder = new(
            connectionString: connectionString);

        string databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            return true;
        }

        builder.InitialCatalog = "master";
        builder.ConnectTimeout = Math.Min(val1: builder.ConnectTimeout, val2: 2);

        using CancellationTokenSource timeout =
            CancellationTokenSource.CreateLinkedTokenSource(token: cancellationToken);

        timeout.CancelAfter(delay: TimeSpan.FromSeconds(seconds: 2));

        try
        {
            await using SqlConnection connection = new(
                connectionString: builder.ConnectionString);

            await connection.OpenAsync(cancellationToken: timeout.Token);
            await using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT DB_ID(@databaseName)";

            command.Parameters.AddWithValue(
                parameterName: "@databaseName",
                value: databaseName);

            object result = await command.ExecuteScalarAsync(
                cancellationToken: timeout.Token);

            return result is not null and not DBNull;
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}