// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Models;
using cCoder.ContentManagement.Models.OData;
using cCoder.Data.Extensions;
using cCoder.Data.Exposures;
using cCoder.Data.Models;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Configurations;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Exceptions;
using System.Security;
using System.Text.Json;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography;

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
            string contentSecurityPolicyNonce =
                Convert.ToBase64String(
                    inArray: RandomNumberGenerator.GetBytes(
                        count: 32));

            context.Items[ContentSecurityPolicyNonceContract.HttpContextItemKey] =
                contentSecurityPolicyNonce;

            context.Response.OnStarting(callback: () =>
            {
                bool allowEditorFraming =
                    context.Request.Query["edit"] == "true";

                context.Response.Headers["X-Content-Type-Options"] =
                    "nosniff";
                context.Response.Headers["Referrer-Policy"] =
                    "no-referrer";
                context.Response.Headers["Content-Security-Policy"] =
                    CreateContentSecurityPolicy(
                        allowEditorFraming: allowEditorFraming,
                        nonce: contentSecurityPolicyNonce);

                if (allowEditorFraming)
                {
                    _ = context.Response.Headers.Remove(
                        key: "X-Frame-Options");
                }
                else
                {
                    context.Response.Headers["X-Frame-Options"] =
                        "DENY";
                }

                _ = context.Response.Headers.Remove(key: "Server");
                _ = context.Response.Headers.Remove(
                    key: "X-Powered-By");

                return Task.CompletedTask;
            });

            await next();
        });

        return app;
    }

    internal static string CreateContentSecurityPolicy(
        bool allowEditorFraming,
        string nonce)
    {
        string policy =
            "default-src 'self'; " +
            "base-uri 'self'; " +
            "object-src 'none'; " +
            "form-action 'self'; " +
            "img-src 'self' data: blob:; " +
            "font-src 'self' data:; " +
            $"style-src 'self' 'nonce-{nonce}'; " +
            "style-src-attr 'unsafe-inline'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            "script-src-attr 'none'; " +
            "connect-src 'self'; " +
            "frame-src 'self';";

        return allowEditorFraming
            ? policy
            : policy + " frame-ancestors 'none';";
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
        CreateSecurityExtendedMetadataContainer(type: typeof(TEntity));

    private static ExtendedMetadataContainer CreateSecurityExtendedMetadataContainer(
        Type type)
    {
        bool isValueType = type.IsValueType || type == typeof(string);

        return new ExtendedMetadataContainer
        {
            IsValueType = isValueType,
            Type = GetSecurityMetadataTypeName(type: type),
            Name = type.Name,
            DisplayName = type.Name,
            Description = type.Name,
            ServerType = type.AssemblyQualifiedName,
            ServerTypeName = GetSecurityCSharpTypeName(type: type),
            Properties = isValueType
                ? []
                : type.GetProperties()
                    .Select(selector: CreateSecurityPropertyContainer)
                    .ToArray(),
            IsEntity = true,
            IsJoinEntity = type.IsJoinType(),
            HasEndpoint = true,
            Category = SecurityMetadataScope,
        };
    }

    private static PropertyContainer CreateSecurityPropertyContainer(
        PropertyInfo property) =>
        new()
        {
            Name = property.Name,
            Type = GetSecurityMetadataTypeName(type: property.PropertyType),
            ServerType = property.PropertyType.ToString(),
            ServerTypeName = GetSecurityCSharpTypeName(type: property.PropertyType),
            IsValueType = property.PropertyType.IsValueType
                || property.PropertyType == typeof(string),
            DisplayName = property.Name,
            ShortDisplayName = property.Name,
            Description = property.Name,
            IsReadOnly = !property.CanWrite,
            Template = property.GetCustomAttribute<KeyAttribute>() is not null
                || property.Name == "Id"
                    ? "key"
                    : property.Name,
            IsRequired = (!(property.PropertyType.IsGenericType
                    && property.PropertyType.GetGenericTypeDefinition()
                        == typeof(Nullable<>))
                && property.PropertyType.IsValueType)
                || property.GetCustomAttribute<RequiredAttribute>() is not null,
        };

    private static string GetSecurityCSharpTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        IEnumerable<string> genericNames = type.GenericTypeArguments
            .Select(selector: GetSecurityCSharpTypeName);

        return $"{type.Name.Split(separator: '`')[0]}<{string.Join(separator: ",", values: genericNames)}>"
            .Replace(oldValue: "System.Object", newValue: "dynamic");
    }

    private static string GetSecurityMetadataTypeName(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }

        if (typeof(IEnumerable).IsAssignableFrom(c: type))
        {
            return "array";
        }

        return Type.GetTypeCode(type: Nullable.GetUnderlyingType(nullableType: type) ?? type) switch
        {
            TypeCode.Boolean => "bool",
            TypeCode.DateTime => "date",
            TypeCode.Decimal => "number",
            TypeCode.Double => "number",
            TypeCode.Int16 => "number",
            TypeCode.Int32 => "number",
            TypeCode.Int64 => "number",
            TypeCode.Byte => "number",
            TypeCode.SByte => "number",
            TypeCode.Single => "number",
            TypeCode.UInt16 => "number",
            TypeCode.UInt32 => "number",
            TypeCode.UInt64 => "number",
            _ when type == typeof(Guid) || type == typeof(Guid?) => "guid",
            _ when type == typeof(TimeSpan) || type == typeof(TimeSpan?) => "time",
            _ when type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?) => "date",
            _ => "object",
        };
    }
}