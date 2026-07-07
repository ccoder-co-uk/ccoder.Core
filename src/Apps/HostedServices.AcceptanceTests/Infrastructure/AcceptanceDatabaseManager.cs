using cCoder.Data;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal sealed class AcceptanceDatabaseManager(
    IServiceProvider services,
    string coreConnectionString,
    string ssoConnectionString)
{
    public Task ResetDatabasesAsync()
    {
        EnsureSafeAcceptanceDatabase(ssoConnectionString, "dev-Members");
        EnsureSafeAcceptanceDatabase(coreConnectionString, "dev-Core");

        ForceDropDatabase(ssoConnectionString);
        ForceDropDatabase(coreConnectionString);

        using IServiceScope scope = services.CreateScope();
        using var sso = scope.ServiceProvider.GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);
        using var core = scope.ServiceProvider.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        sso.Migrate();
        core.Migrate();

        return Task.CompletedTask;
    }

    public Task MigrateDatabasesAsync()
    {
        EnsureSafeAcceptanceDatabase(ssoConnectionString, "dev-Members");
        EnsureSafeAcceptanceDatabase(coreConnectionString, "dev-Core");

        using IServiceScope scope = services.CreateScope();
        using var sso = scope.ServiceProvider.GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);
        using var core = scope.ServiceProvider.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

        sso.Migrate();
        core.Migrate();

        return Task.CompletedTask;
    }

    public Task DropDatabasesAsync()
    {
        EnsureSafeAcceptanceDatabase(ssoConnectionString, "dev-Members");
        EnsureSafeAcceptanceDatabase(coreConnectionString, "dev-Core");

        ForceDropDatabase(ssoConnectionString);
        ForceDropDatabase(coreConnectionString);

        return Task.CompletedTask;
    }

    private void ForceDropDatabase(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        SqlConnectionStringBuilder builder = CreateAcceptanceConnectionStringBuilder(connectionString);
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName))
            return;

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
        _ = command.Parameters.AddWithValue("@databaseName", databaseName);
        command.ExecuteNonQuery();
    }

    private static void EnsureSafeAcceptanceDatabase(string connectionString, string protectedDatabaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Acceptance database connection string is empty.");

        SqlConnectionStringBuilder builder = CreateAcceptanceConnectionStringBuilder(connectionString);
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Acceptance database name is empty.");

        if (databaseName.Equals(protectedDatabaseName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Refusing to run acceptance database operations against protected database '{protectedDatabaseName}'.");

        if (!databaseName.Contains("accept", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Refusing to run acceptance database operations against non-acceptance database '{databaseName}'.");
    }

    private static SqlConnectionStringBuilder CreateAcceptanceConnectionStringBuilder(string connectionString) =>
        new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true,
        };
}
