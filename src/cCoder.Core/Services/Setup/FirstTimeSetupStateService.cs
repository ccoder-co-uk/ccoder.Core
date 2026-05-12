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
            await using DbContext sso = securityDbContextFactory.CreateDbContext(true);

            if (!await DatabaseExistsAsync(core, cancellationToken)
                || !await DatabaseExistsAsync(sso, cancellationToken))
            {
                return false;
            }

            bool hasApp = await core.Set<App>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken);

            if (!hasApp)
                return false;

            bool hasRootPage = await core.Set<Page>()
                .IgnoreQueryFilters()
                .AnyAsync(page => page.Path == string.Empty, cancellationToken);

            if (!hasRootPage)
                return false;

            bool hasCommonObjects = await core.Set<CommonObject>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken);

            if (!hasCommonObjects)
                return false;

            bool hasFiles = await core.Set<cCoder.Data.Models.DMS.File>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken);

            if (!hasFiles)
                return false;

            return await sso.Set<Tenant>()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken);
        }
        catch (Exception ex) when (IsSetupDatabaseException(ex))
        {
            log.LogInformation(
                "First-time setup is available because one or more platform databases are not ready. {Message}",
                ex.Message);

            return false;
        }
    }

    private static async Task<bool> DatabaseExistsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        string connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        SqlConnectionStringBuilder builder = new(connectionString);
        string databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            return true;

        builder.InitialCatalog = "master";
        builder.ConnectTimeout = Math.Min(builder.ConnectTimeout, 2);

        using CancellationTokenSource timeout = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(timeout.Token);
            await using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT DB_ID(@databaseName)";
            command.Parameters.AddWithValue("@databaseName", databaseName);

            object result = await command.ExecuteScalarAsync(timeout.Token);
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
            SqlException sqlException => IsDatabaseUnavailable(sqlException),
            _ when ex.InnerException is not null => IsSetupDatabaseException(ex.InnerException),
            _ => false,
        };

    private static bool IsDatabaseUnavailable(SqlException ex) =>
        ex.Errors.OfType<SqlError>().Any(error => error.Number is 208 or 4060 or 911);
}
