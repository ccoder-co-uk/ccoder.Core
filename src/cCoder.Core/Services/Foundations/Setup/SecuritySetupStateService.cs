// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Setup;
using cCoder.Security.Objects.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class SecuritySetupStateService(
    ISecuritySetupContextBroker securitySetupContextBroker)
    : ISecuritySetupStateService
{
    public ValueTask<bool> IsSecurityInitializedAsync(
        CancellationToken cancellationToken = default) =>
        TryCatch(operation: async () =>
        {
            ValidateCancellationTokenOnCheck(
                cancellationToken: cancellationToken);

            await using DbContext securityContext =
                securitySetupContextBroker.CreateSecurityContext();

            if (!await DatabaseExistsAsync(
                context: securityContext,
                cancellationToken: cancellationToken))
            {
                return false;
            }

            return await securityContext.Set<Tenant>()
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