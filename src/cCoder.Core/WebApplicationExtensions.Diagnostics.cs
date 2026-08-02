// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using System.Web;
using cCoder.Data;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Models;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Dependencies;
using cCoder.Security.Models.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static async Task HandleUnhandledException(HttpContext context)
    {
        ILogger logger = context.RequestServices
            .GetService<ILoggerFactory>()?
            .CreateLogger(categoryName: "cCoder.Core.Web")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        Exception exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.StatusCode =
            exception?.GetType() == typeof(SecurityException) ? 401 : 500;

        context.Response.ContentType = "application/json";

        if (exception is null)
        {
            return;
        }

        logger.LogError("{Message}\n{StackTrace}", exception.Message, exception.StackTrace);

        await context.Response.WriteAsync(
text: "{ \"error\": \"" + exception.Message.Replace(oldValue: "\"", newValue: "\'") + "\" }");

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
            || request.Path.StartsWithSegments(other: "/Api/Hubs", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CoreConfiguration configuration =
            context.RequestServices.GetRequiredService<CoreConfiguration>();
        string ssoUserId = "Guest";

        string url = HttpUtility.UrlDecode(str: request.GetDisplayUrl());

        string logEntry =
            $"{context.Connection.RemoteIpAddress} as {ssoUserId}: {request.Method} - {url}";

        string ssoConnectionString =
            configuration.Security?.ConnectionString;

        if (!string.IsNullOrWhiteSpace(value: ssoConnectionString))
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

                tenantId = appService.GetAppByDomain(
                    domain: request.Host.Host,
                    ignoreFilters: true)?.TenantId;

                using SecurityDbContext sso = new MSSQLSecurityDbContextFactory(ssoConnectionString)
                    .CreateDbContext();

                string existingUserId = await sso.Set<SSOUser>()
                    .IgnoreQueryFilters()
                    .Where(predicate: user => user.Id == ssoUserId)
                    .Select(selector: user => user.Id)
                    .FirstOrDefaultAsync(cancellationToken: context.RequestAborted);

                string existingTenantId = string.IsNullOrWhiteSpace(value: tenantId)
                    ? null
                    : await sso.Set<Tenant>()
                        .IgnoreQueryFilters()
                        .Where(predicate: tenant => tenant.Id == tenantId)
                        .Select(selector: tenant => tenant.Id)
                        .FirstOrDefaultAsync(cancellationToken: context.RequestAborted);

                string requestType =
                    request.Path.Value?.StartsWith(value: "/api/", comparisonType: StringComparison.InvariantCultureIgnoreCase) == true
                        ? "Api_"
                        : "Page_";

                UserEvent userEvent = new()
                {
                    TenantId = existingTenantId,
                    CreatedBy = existingUserId,
                    EventName = $"{requestType}{request.Method}{request.Path.Value}",
                    CreatedOn = DateTimeOffset.UtcNow,
                    Value = url,
                };

                await sso.AddAsync(entity: userEvent);
                await sso.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(
message: "Unable to persist request log entry to SSO. {Message}", args: ex.Message);
            }
        }

        if (logger.IsEnabled(logLevel: LogLevel.Debug))
        {
            logger.LogDebug(
                message: "Request diagnostics: {LogEntry}",
                args: logEntry);
        }
    }

}
