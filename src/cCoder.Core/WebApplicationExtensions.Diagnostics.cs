using System.Security;
using System.Web;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Objects.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Data.SqlClient;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static async Task HandleUnhandledException(HttpContext context)
    {
        ILogger logger = context.RequestServices
            .GetService<ILoggerFactory>()?
            .CreateLogger("cCoder.Core.Web")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        Exception exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.StatusCode =
            exception?.GetType() == typeof(SecurityException) ? 401 : 500;

        context.Response.ContentType = "application/json";

        if (exception is null)
            return;

        logger.LogError("{Message}\n{StackTrace}", exception.Message, exception.StackTrace);
        await context.Response.WriteAsync(
            "{ \"error\": \"" + exception.Message.Replace("\"", "\'") + "\" }");

        Exception innerException = exception.InnerException;

        while (innerException is not null)
        {
            logger.LogError("{Message}\n{StackTrace}", innerException.Message, innerException.StackTrace);
            innerException = innerException.InnerException;
        }
    }

    private static async Task LogRequest(HttpContext context, ILogger logger)
    {
        HttpRequest request = context.RequestServices.GetService<HttpRequest>();

        if (request is null
            || request.Path.StartsWithSegments("/Api/Hubs", StringComparison.OrdinalIgnoreCase))
            return;

        Config config = context.RequestServices.GetRequiredService<Config>();
        string ssoUserId = "Guest";

        string url = HttpUtility.UrlDecode(request.GetDisplayUrl());
        string logEntry =
            $"{context.Connection.RemoteIpAddress} as {ssoUserId}: {request.Method} - {url}";

        if (config.ConnectionStrings?.TryGetValue("SSO", out string ssoConnectionString) == true
            && !string.IsNullOrWhiteSpace(ssoConnectionString)
            && await SqlTableExistsAsync(ssoConnectionString, "dbo", "Sessions", context.RequestAborted)
            && await SqlTableExistsAsync(ssoConnectionString, "dbo", "UserEvents", context.RequestAborted))
        {
            try
            {
                ICoreAuthInfo authInfo = context.RequestServices.GetRequiredService<ICoreAuthInfo>();
                IContentManagementAppService appService =
                    context.RequestServices.GetRequiredService<IContentManagementAppService>();

                ssoUserId = authInfo.SSOUserId ?? "Guest";
                logEntry =
                    $"{context.Connection.RemoteIpAddress} as {ssoUserId}: {request.Method} - {url}";

                string tenantId = null;

                if (config.ConnectionStrings?.TryGetValue("Core", out string coreConnectionString) == true
                    && await SqlTableExistsAsync(coreConnectionString, "CMS", "Apps", context.RequestAborted))
                {
                    tenantId = appService.GetByDomain(request.Host.Host, ignoreFilters: true)?.TenantId;
                }

                using SecurityDbContext sso = new MSSQLSecurityDbContextFactory(ssoConnectionString)
                    .CreateDbContext();

                string requestType =
                    request.Path.Value?.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase) == true
                        ? "Api_"
                        : "Page_";

                UserEvent userEvent = new()
                {
                    TenantId = tenantId,
                    CreatedBy = ssoUserId,
                    EventName = $"{requestType}{request.Method}{request.Path.Value}",
                    CreatedOn = DateTimeOffset.UtcNow,
                    Value = url,
                };

                await sso.AddAsync(userEvent);
                await sso.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    "Unable to persist request log entry to SSO. {Message}",
                    ex.Message);
            }
        }

        logger.LogDebug(logEntry);
    }

    private static async Task<bool> SqlTableExistsAsync(
        string connectionString,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        try
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                ConnectTimeout = 2,
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT OBJECT_ID(@tableName, 'U')";
            command.Parameters.AddWithValue("@tableName", $"{schema}.{table}");

            object result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null and not DBNull;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
