using cCoder.Core.Cors;
using cCoder.Data;
using cCoder.Logging.Exposures.Hubs;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    public static WebApplication StartCoreWeb(this WebApplication app)
    {
        ILogger log = app.Services
            .GetService<ILoggerFactory>()?
            .CreateLogger("cCoder.Core.Web")
            ?? NullLogger.Instance;

        app.EnsureCoreDatabasesMigrated(log);
        app.Services.GetRequiredService<ICoreAllowedOriginStore>()
            .RefreshAsync()
            .GetAwaiter()
            .GetResult();

        app.UseHttpsRedirection();
        app.UseCoreApi(log);

        return app;
    }

    public static WebApplication StartCoreHostedServices(this WebApplication app)
    {
        ILogger log = app.Services
            .GetService<ILoggerFactory>()?
            .CreateLogger("cCoder.Core.HostedServices")
            ?? NullLogger.Instance;

        app.EnsureCoreDatabasesMigrated(log);
        app.Services.GetRequiredService<ICoreAllowedOriginStore>()
            .RefreshAsync()
            .GetAwaiter()
            .GetResult();

        app.ListenToExternalEvents();
        app.UseRouting();
        app.UseAuthorization();
        app.UseCoreDefaultCors();
        app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
        });
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                if (context.Request.Query["edit"] != "true")
                    context.Response.Headers.Append("X-Frame-Options", "DENY");

                _ = context.Response.Headers.Remove("X-AspNet-Version");
                _ = context.Response.Headers.Remove("X-AspNetMvc-Version");
                _ = context.Response.Headers.Remove("X-Sourcefiles");
                _ = context.Response.Headers.Remove("Server");

                return Task.CompletedTask;
            });
            await next();
        });
        app.MapControllers();
        app.MapHub<LogHub>("/Hubs/Logs");
        return app;
    }

    private static void EnsureCoreDatabasesMigrated(this WebApplication app, ILogger log = null)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ICoreContextFactory coreContextFactory =
            scope.ServiceProvider.GetRequiredService<ICoreContextFactory>();
        ISecurityDbContextFactory securityDbContextFactory =
            scope.ServiceProvider.GetRequiredService<ISecurityDbContextFactory>();

        using CoreDataContext coreContext = coreContextFactory.CreateCoreContext();
        using SecurityDbContext securityContext = securityDbContextFactory.CreateDbContext(true);

        string coreConnectionString = coreContext.Database.GetConnectionString();
        string securityConnectionString = securityContext.Database.GetConnectionString();

        log?.LogInformation(
            "Applying startup database migrations. Core={CoreDatabase}; Security={SecurityDatabase}",
            ResolveDatabaseName(coreConnectionString),
            ResolveDatabaseName(securityConnectionString));

        using IDisposable migrationLock =
            AcquireStartupMigrationLock(coreConnectionString, securityConnectionString, log);

        securityContext.Migrate();
        coreContext.Migrate();
    }

    private static IDisposable AcquireStartupMigrationLock(
        string coreConnectionString,
        string securityConnectionString,
        ILogger log)
    {
        string lockName = BuildStartupMigrationLockName(coreConnectionString, securityConnectionString);
        Mutex mutex = new(false, lockName);

        try
        {
            if (!mutex.WaitOne(TimeSpan.FromMinutes(2)))
                throw new TimeoutException(
                    $"Timed out waiting for startup migration lock '{lockName}'.");
        }
        catch (AbandonedMutexException)
        {
            log?.LogWarning(
                "Recovered abandoned startup migration lock {LockName}. Continuing with database migration.",
                lockName);
        }

        log?.LogDebug("Acquired startup migration lock {LockName}.", lockName);
        return new StartupMigrationLock(mutex, lockName, log);
    }

    private static string BuildStartupMigrationLockName(
        string coreConnectionString,
        string securityConnectionString)
    {
        string lockKey = string.Join(
            "|",
            ResolveDatabaseName(coreConnectionString),
            ResolveDatabaseName(securityConnectionString));

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(lockKey));
        return $"Global\\cCoder.Core.StartupMigrate.{Convert.ToHexString(hash)}";
    }

    private static string ResolveDatabaseName(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "(none)";

        try
        {
            SqlConnectionStringBuilder builder = new(connectionString);
            return string.IsNullOrWhiteSpace(builder.InitialCatalog)
                ? "(default)"
                : builder.InitialCatalog;
        }
        catch
        {
            return "(unparsed)";
        }
    }

    private sealed class StartupMigrationLock(Mutex mutex, string lockName, ILogger log) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                mutex.ReleaseMutex();
                log?.LogDebug("Released startup migration lock {LockName}.", lockName);
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}
