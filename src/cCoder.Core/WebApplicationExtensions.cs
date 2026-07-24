// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Logging;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static readonly ConditionalWeakTable<WebApplication, object> StartedCoreWebApps = new();
    private static readonly object StartedCoreWebAppsLock = new();

    public static WebApplication StartCoreWeb(this WebApplication app)
    {
        if (!TryMarkCoreWebStarted(app: app))
        {
            return app;
        }

        ILogger log = app.Services
            .GetService<ILoggerFactory>()?
            .CreateLogger(categoryName: "cCoder.Core.Web")
            ?? NullLogger.Instance;

        app.EnsureCoreDatabasesMigrated(log: log);
        app.UseHttpsRedirection();
        app.UseCoreApi(log: log);

        return app;
    }

    private static bool TryMarkCoreWebStarted(WebApplication app)
    {
        lock (StartedCoreWebAppsLock)
        {
            if (StartedCoreWebApps.TryGetValue(key: app, value: out _))
            {
                return false;
            }

            StartedCoreWebApps.Add(key: app, value: new object());
            return true;
        }
    }

    public static WebApplication StartCoreHostedServices(this WebApplication app)
    {
        ILogger log = app.Services
            .GetService<ILoggerFactory>()?
            .CreateLogger(categoryName: "cCoder.Core.HostedServices")
            ?? NullLogger.Instance;

        app.EnsureCoreDatabasesMigrated(log: log);
        IHostedService[] hostedServices = app.Services.GetServices<IHostedService>()
            .ToArray();
        log.LogInformation(
message: "Registered hosted services: {HostedServices}", args: string.Join(separator: ", ", values: hostedServices.Select(selector: service => service.GetType().FullName)));

        app.ListenToExternalEvents();
        app.UseRouting();
        app.UseAuthorization();
        app.UseCoreDefaultCors();
        app.UseStaticFiles(options: new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
        });
        app.Use(middleware: async (context, next) =>
        {
            context.Response.OnStarting(callback: () =>
            {
                if (context.Request.Query["edit"] != "true")
                {
                    context.Response.Headers.Append(key: "X-Frame-Options", value: "DENY");
                }

                _ = context.Response.Headers.Remove(key: "X-AspNet-Version");
                _ = context.Response.Headers.Remove(key: "X-AspNetMvc-Version");
                _ = context.Response.Headers.Remove(key: "X-Sourcefiles");
                _ = context.Response.Headers.Remove(key: "Server");

                return Task.CompletedTask;
            });
            await next();
        });
        app.MapControllers();
        app.StartLoggingWeb(log: log);
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
        using SecurityDbContext securityContext = securityDbContextFactory.CreateDbContext(ignoreAuthInfo: true);

        string coreConnectionString = coreContext.Database.GetConnectionString();
        string securityConnectionString = securityContext.Database.GetConnectionString();

        log?.LogInformation(
            "Applying startup database migrations. Core={CoreDatabase}; Security={SecurityDatabase}",
            ResolveDatabaseName(coreConnectionString),
            ResolveDatabaseName(securityConnectionString));

        using IDisposable migrationLock =
            AcquireStartupMigrationLock(coreConnectionString: coreConnectionString, securityConnectionString: securityConnectionString, log: log);

        securityContext.Migrate();
        coreContext.Migrate();
    }

    private static IDisposable AcquireStartupMigrationLock(
        string coreConnectionString,
        string securityConnectionString,
        ILogger log)
    {
        string lockName = BuildStartupMigrationLockName(coreConnectionString: coreConnectionString, securityConnectionString: securityConnectionString);
        Mutex mutex = new(false, lockName);

        try
        {
            if (!mutex.WaitOne(timeout: TimeSpan.FromMinutes(minutes: 2)))
            {
                throw new TimeoutException(
                    $"Timed out waiting for startup migration lock '{lockName}'.");
            }
        }
        catch (AbandonedMutexException)
        {
            log?.LogWarning(
message: "Recovered abandoned startup migration lock {LockName}. Continuing with database migration.", args: lockName);
        }

        log?.LogDebug(message: "Acquired startup migration lock {LockName}.", args: lockName);
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

        byte[] hash = SHA256.HashData(source: Encoding.UTF8.GetBytes(s: lockKey));
        return $"Global\\cCoder.Core.StartupMigrate.{Convert.ToHexString(inArray: hash)}";
    }

    private static string ResolveDatabaseName(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            return "(none)";
        }

        try
        {
            SqlConnectionStringBuilder builder = new(connectionString);
            return string.IsNullOrWhiteSpace(value: builder.InitialCatalog)
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
                log?.LogDebug(message: "Released startup migration lock {LockName}.", args: lockName);
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}