// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core;

internal static class SetupDatabase
{
    private static readonly TimeSpan DatabaseTimeout =
        TimeSpan.FromSeconds(seconds: 2);

    internal static async ValueTask<bool> IsInitializedAsync<TEntity>(
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

        timeout.CancelAfter(delay: DatabaseTimeout);

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