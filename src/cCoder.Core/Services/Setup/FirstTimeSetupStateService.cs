// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupStateService(
    ICoreContextFactory coreContextFactory,
    ISecurityDbContextFactory securityDbContextFactory,
    ILogger<FirstTimeSetupStateService> log) : IFirstTimeSetupStateService
{
    public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using DbContext core = coreContextFactory.CreateCoreContext();
            await using DbContext sso = securityDbContextFactory.CreateDbContext(ignoreAuthInfo: true);

            if (!await DatabaseExistsAsync(context: core, cancellationToken: cancellationToken)
                || !await DatabaseExistsAsync(context: sso, cancellationToken: cancellationToken))
            {
                return false;
            }

            bool hasApp = await core.Set<App>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken: cancellationToken);

            if (!hasApp)
            {
                return false;
            }

            return await sso.Set<Tenant>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (IsSetupDatabaseException(ex: ex))
        {
            log.LogInformation(
message: "First-time setup is available because one or more platform databases are not ready. {Message}", args: ex.Message);

            return false;
        }
    }

    private static async Task<bool> DatabaseExistsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        string connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            return false;
        }

        SqlConnectionStringBuilder builder = new(connectionString);
        string databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            return true;
        }

        builder.InitialCatalog = "master";
        builder.ConnectTimeout = Math.Min(val1: builder.ConnectTimeout, val2: 2);

        using CancellationTokenSource timeout = CancellationTokenSource
            .CreateLinkedTokenSource(token: cancellationToken);

        timeout.CancelAfter(delay: TimeSpan.FromSeconds(seconds: 2));

        try
        {
            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken: timeout.Token);
            await using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT DB_ID(@databaseName)";
            command.Parameters.AddWithValue(parameterName: "@databaseName", value: databaseName);

            object result = await command.ExecuteScalarAsync(cancellationToken: timeout.Token);
            return result is not null and not DBNull;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private static bool IsSetupDatabaseException(Exception ex) =>
        ex switch
        {
            SqlException sqlException => IsDatabaseUnavailable(ex: sqlException),
            _ when ex.InnerException is not null => IsSetupDatabaseException(ex: ex.InnerException),
            _ => false,
        };

    private static bool IsDatabaseUnavailable(SqlException ex) =>
        ex.Errors.OfType<SqlError>()
            .Any(predicate: error => error.Number is 208 or 4060 or 911);
}