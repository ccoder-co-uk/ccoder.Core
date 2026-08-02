// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models.OData;
using cCoder.Data.Exposures;
using cCoder.Data.Models;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Configurations;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Exceptions;
using System.Security;
using System.Text.Json;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private const string SecurityMetadataScope = "Security";

    private static WebApplication UseCoreSecurityHeaders(
        this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.Use(middleware: async (context, next) =>
        {
            context.Response.OnStarting(callback: () =>
            {
                context.Response.Headers["X-Content-Type-Options"] =
                    "nosniff";
                context.Response.Headers["Referrer-Policy"] =
                    "no-referrer";

                _ = context.Response.Headers.Remove(key: "Server");
                _ = context.Response.Headers.Remove(
                    key: "X-Powered-By");

                return Task.CompletedTask;
            });

            await next();
        });

        return app;
    }

    private static WebApplication UseCoreMetadataAuthorization(
        this WebApplication app)
    {
        Models.CoreConfiguration configuration =
            app.Services.GetService<Models.CoreConfiguration>();

        bool exposeApiMetadata =
            ShouldExposeApiSurface(
                configuredValue:
                    configuration?.Api?.ExposeMetadata,
                isProduction: app.Environment.IsProduction());

        bool exposeApiDocumentation =
            ShouldExposeApiSurface(
                configuredValue:
                    configuration?.Api?.ExposeDocumentation,
                isProduction: app.Environment.IsProduction());

        HashSet<string> oDataServiceDocumentPaths =
            new(
                collection: app.Services
                    .GetServices<ApiInfo>()
                    .Where(predicate: info => string.Equals(
                        a: info.Kind,
                        b: "Context",
                        comparisonType:
                            StringComparison.OrdinalIgnoreCase))
                    .Select(selector: info =>
                        $"/Api/{info.Name}"),
                comparer: StringComparer.OrdinalIgnoreCase);

        app.Use(middleware: async (context, next) =>
        {
            bool isMetadataRequest =
                context.Request.Path.Value?.EndsWith(
                    value: "/$metadata",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase) == true;

            bool isODataServiceDocumentRequest =
                oDataServiceDocumentPaths.Contains(
                    item: context.Request.Path.Value?
                        .TrimEnd(trimChar: '/')
                        ?? string.Empty);

            bool isDocumentationRequest =
                context.Request.Path.StartsWithSegments(
                    other: "/swagger",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase);

            if (((isMetadataRequest
                    || isODataServiceDocumentRequest)
                    && !exposeApiMetadata)
                || (isDocumentationRequest
                    && !exposeApiDocumentation))
            {
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                return;
            }

            if (((isMetadataRequest
                    || isODataServiceDocumentRequest)
                    && exposeApiMetadata)
                || (isDocumentationRequest
                    && exposeApiDocumentation))
            {
                bool isAuthorized =
                    AuthorizeApiMetadataRequest(
                        context: context);

                if (!isAuthorized)
                {
                    return;
                }

                context.Response.Headers.CacheControl =
                    "private, no-store";
                context.Response.Headers.Pragma = "no-cache";
            }

            await next();
        });

        return app;
    }

    internal static bool AuthorizeApiMetadataRequest(
        HttpContext context)
    {
        ISSOAuthInfo authentication =
            context.RequestServices
                .GetService<ISSOAuthInfo>();

        if (authentication is null
            || string.IsNullOrWhiteSpace(
                value: authentication.SSOUserId)
            || string.Equals(
                a: authentication.SSOUserId,
                b: "Guest",
                comparisonType:
                    StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate =
                "Bearer";

            return false;
        }

        try
        {
            IApiMetadataAuthorizationManager
                authorizationManager =
                context.RequestServices
                    .GetRequiredService<
                        IApiMetadataAuthorizationManager>();

            authorizationManager
                .EnsureUserCanReadApiMetadata();

            return true;
        }
        catch (SecurityException)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return false;
        }
        catch (SecurityServiceException exception)
            when (exception.GetBaseException()
                is SecurityException)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return false;
        }
    }

    internal static bool ShouldExposeApiSurface(
        bool? configuredValue,
        bool isProduction) =>
        configuredValue ?? !isProduction;

    private static void PopulateSecurityMetadataTypeCache(
        this WebApplication app)
    {
        IMetadataTypeCache metadataTypeCache =
            app.Services.GetRequiredService<IMetadataTypeCache>();

        if (metadataTypeCache.Contains(
            scope: SecurityMetadataScope))
        {
            return;
        }

        metadataTypeCache.Set(
            scope: SecurityMetadataScope,
            typeSetPayloads:
            [
                JsonSerializer.Serialize(
                    value: new MetadataContainerSet
                    {
                        Name = SecurityMetadataScope,
                        UriBase = SecurityMetadataScope,
                        Types =
                        [
                            SecurityEntity<SSOUser>(),
                            SecurityEntity<SSORole>(),
                            SecurityEntity<SSOPrivilege>(),
                            SecurityEntity<Tenant>(),
                            SecurityEntity<TenantAnalysis>(),
                            SecurityEntity<UserEvent>(),
                            SecurityEntity<SSOUserRole>(),
                        ],
                    })
            ]);
    }

    private static ExtendedMetadataContainer SecurityEntity<TEntity>() =>
        new(
            type: typeof(TEntity),
            isEntity: true,
            hasEndpoint: true)
        {
            Category = SecurityMetadataScope,
        };
}