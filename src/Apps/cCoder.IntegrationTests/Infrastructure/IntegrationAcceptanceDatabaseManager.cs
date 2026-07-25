// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class IntegrationAcceptanceDatabaseManager(
    IServiceProvider services,
    string coreConnectionString,
    string ssoConnectionString)
{
    public Task ResetDatabasesAsync()
    {
        EnsureSafeAcceptanceDatabase(connectionString: ssoConnectionString,protectedDatabaseName: "dev-Members");
        EnsureSafeAcceptanceDatabase(connectionString: coreConnectionString,protectedDatabaseName: "dev-Core");

        ForceDropDatabase(connectionString: ssoConnectionString);
        ForceDropDatabase(connectionString: coreConnectionString);

        using IServiceScope scope = services.CreateScope();

        using var sso = scope.ServiceProvider.GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        using var core = scope.ServiceProvider.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        sso.Migrate();
        core.Migrate();

        return Task.CompletedTask;
    }

    public Task DropDatabasesAsync()
    {
        EnsureSafeAcceptanceDatabase(connectionString: ssoConnectionString,protectedDatabaseName: "dev-Members");
        EnsureSafeAcceptanceDatabase(connectionString: coreConnectionString,protectedDatabaseName: "dev-Core");

        ForceDropDatabase(connectionString: ssoConnectionString);
        ForceDropDatabase(connectionString: coreConnectionString);

        return Task.CompletedTask;
    }

    private void ForceDropDatabase(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            return;
        }

        SqlConnectionStringBuilder builder = CreateAcceptanceConnectionStringBuilder(connectionString: connectionString);
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            return;
        }

        builder.InitialCatalog = "master";

        using SqlConnection connection = new(builder.ConnectionString);
        connection.Open();

        using SqlCommand command = connection.CreateCommand();

        command.CommandText = @"
IF DB_ID(@databaseName) IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) =
        N'ALTER DATABASE [' + REPLACE(@databaseName, ']', ']]') + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;'
        + N'DROP DATABASE [' + REPLACE(@databaseName, ']', ']]') + N']';
    EXEC(@sql);
END";

        _ = command.Parameters.AddWithValue(parameterName: "@databaseName",value: databaseName);
        command.ExecuteNonQuery();
    }

    private static void EnsureSafeAcceptanceDatabase(string connectionString, string protectedDatabaseName)
    {
        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            throw new InvalidOperationException("Acceptance database connection string is empty.");
        }

        SqlConnectionStringBuilder builder = CreateAcceptanceConnectionStringBuilder(connectionString: connectionString);
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            throw new InvalidOperationException("Acceptance database name is empty.");
        }

        if (databaseName.Equals(value: protectedDatabaseName,comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run acceptance database operations against protected database '{protectedDatabaseName}'.");
        }

        if (!databaseName.Contains(value: "accept",comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run acceptance database operations against non-acceptance database '{databaseName}'.");
        }
    }

    private static SqlConnectionStringBuilder CreateAcceptanceConnectionStringBuilder(string connectionString) =>
        new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true
        };
}