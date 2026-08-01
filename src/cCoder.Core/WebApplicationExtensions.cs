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
using ClientRelationshipManagement.Web.Services.Migration;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static readonly ConditionalWeakTable<WebApplication, object> StartedCoreWebApps = [];
    private static readonly Lock StartedCoreWebAppsLock = new();

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
        app.EnsureCrmDatabaseInitialised();
        app.UseHttpsRedirection();
        app.UseCoreSecurityHeaders();
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
        app.EnsureCrmDatabaseInitialised();
        app.UseCoreSecurityHeaders();

        IHostedService[] hostedServices = [.. app.Services.GetServices<IHostedService>()];

        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.LogInformation(
                message: "Registered hosted services: {HostedServices}",
                args: string.Join(
                    separator: ", ",
                    values: hostedServices.Select(
                        selector: service => service.GetType().FullName)));
        }

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

        Models.CoreConfiguration configuration =
            scope.ServiceProvider.GetRequiredService<Models.CoreConfiguration>();

        ICoreContextFactory coreContextFactory =
            scope.ServiceProvider.GetRequiredService<ICoreContextFactory>();

        using CoreDataContext coreContext = coreContextFactory.CreateCoreContext();

        string coreConnectionString = coreContext.Database.GetConnectionString();
        string securityConnectionString = null;
        SecurityDbContext securityContext = null;

        if (configuration.Security is not null)
        {
            ISecurityDbContextFactory securityDbContextFactory =
                scope.ServiceProvider
                    .GetRequiredService<ISecurityDbContextFactory>();

            securityContext =
                securityDbContextFactory.CreateDbContext(
                    ignoreAuthInfo: true);

            securityConnectionString =
                securityContext.Database.GetConnectionString();

        }

        using (securityContext)
        {
            using IDisposable migrationLock =
                AcquireStartupMigrationLock(
                    coreConnectionString: coreConnectionString,
                    securityConnectionString: securityConnectionString);

            securityContext?.Migrate();
            coreContext.Migrate();
        }

        if (log?.IsEnabled(logLevel: LogLevel.Information) == true)
        {
            log.LogInformation(
                message: "Applied startup database migrations. Core={CoreDatabase}; Security={SecurityDatabase}",
                args:
                [
                    ResolveDatabaseName(connectionString: coreConnectionString),
                    ResolveDatabaseName(connectionString: securityConnectionString)
                ]);
        }
    }

    private static void EnsureCrmDatabaseInitialised(
        this WebApplication app)
    {
        Models.CoreConfiguration configuration =
            app.Services.GetRequiredService<Models.CoreConfiguration>();

        if (configuration.CRM is null)
        {
            return;
        }

        app.Services
            .InitialiseCrmApplicationAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();
    }

    private static IDisposable AcquireStartupMigrationLock(
        string coreConnectionString,
        string securityConnectionString)
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
        }

        return new StartupMigrationLock(mutex);
    }

    private static string BuildStartupMigrationLockName(
        string coreConnectionString,
        string securityConnectionString)
    {
        string lockKey = string.Join(
            separator: "|",
            values:
            [
                ResolveDatabaseName(connectionString: coreConnectionString),
                ResolveDatabaseName(connectionString: securityConnectionString)
            ]);

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

    private sealed class StartupMigrationLock(Mutex mutex) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                mutex.ReleaseMutex();
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}