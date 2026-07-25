// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Setup;
using cCoder.Data.Models.CMS;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class CoreSetupStateService(
    ICoreSetupContextBroker coreSetupContextBroker)
    : ICoreSetupStateService
{
    public ValueTask<bool> IsCoreInitializedAsync(
        CancellationToken cancellationToken = default) =>
        TryCatch(operation: async () =>
        {
            ValidateCancellationTokenOnCheck(
                cancellationToken: cancellationToken);

            await using DbContext coreContext =
                coreSetupContextBroker.CreateCoreContext();

            if (!await DatabaseExistsAsync(
                context: coreContext,
                cancellationToken: cancellationToken))
            {
                return false;
            }

            return await coreContext.Set<App>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken: cancellationToken);
        });

    private static async ValueTask<bool> DatabaseExistsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        string connectionString =
            context.Database.GetConnectionString();

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

        builder.ConnectTimeout = Math.Min(
            val1: builder.ConnectTimeout,
            val2: 2);

        using CancellationTokenSource timeout =
            CancellationTokenSource.CreateLinkedTokenSource(
                token: cancellationToken);

        timeout.CancelAfter(
            delay: TimeSpan.FromSeconds(seconds: 2));

        try
        {
            await using SqlConnection connection = new(
                connectionString: builder.ConnectionString);

            await connection.OpenAsync(
                cancellationToken: timeout.Token);

            await using SqlCommand command =
                connection.CreateCommand();

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