// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Workflow.Activities.Models;
using cCoder.Workflow.Engine;
using cCoder.Workflow.Engine.Exposures;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Workflow;

public class Program
{
    private const int MaximumRequestSize = 1_048_576;

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder =
            WebApplication.CreateBuilder(args: args);

        builder.Services.AddProblemDetails();
        builder.Services.AddWorkflowEngineHostedServices();
        builder.Services.AddHttpClient(name: "WorkflowHttpClientBroker")
            .ConfigurePrimaryHttpMessageHandler(
                () => CreateWebHandler(
                    trustedCertificatePath: builder.Configuration["Workflow:TrustedWebCertificatePath"],
                    useEphemeralTestAuthority: builder.Environment.IsEnvironment(environmentName: "Acceptance")));

        WebApplication app = builder.Build();
        string executionToken = builder.Configuration["Workflow:ExecutionToken"];
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/api/Health", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (!IsAuthorizedExecution(
                authorization: context.Request.Headers.Authorization,
                executionToken: executionToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(context);
        });
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            StatusCodeSelector = exception =>
                exception is BadHttpRequestException badRequest
                    ? badRequest.StatusCode
                    : StatusCodes.Status500InternalServerError
        });

        app.MapMethods(
            pattern: "/api/Health",
            httpMethods: ["GET", "HEAD"],
            handler: () => Results.Text(content: "OK"));

        app.MapPost(
            pattern: "/api/Execute",
            handler: ExecuteAsync);

        app.MapPost(
            pattern: "/api/ExecuteScript",
            handler: ExecuteScriptAsync);

        app.Run();
    }

    private static SocketsHttpHandler CreateWebHandler(
        string trustedCertificatePath,
        bool useEphemeralTestAuthority)
    {
        SocketsHttpHandler handler = new() { AllowAutoRedirect = false };

        if (!string.IsNullOrWhiteSpace(trustedCertificatePath))
        {
            X509ChainPolicy policy = new()
            {
                TrustMode = X509ChainTrustMode.CustomRootTrust,
                RevocationMode = useEphemeralTestAuthority
                    ? X509RevocationMode.NoCheck
                    : X509RevocationMode.Online
            };

            policy.CustomTrustStore.Add(
                certificate: X509CertificateLoader.LoadCertificateFromFile(path: trustedCertificatePath));
            policy.ApplicationPolicy.Add(oid: new Oid("1.3.6.1.5.5.7.3.1"));
            handler.SslOptions = new SslClientAuthenticationOptions { CertificateChainPolicy = policy };
        }

        return handler;
    }

    internal static bool IsAuthorizedExecution(string authorization, string executionToken)
    {
        if (string.IsNullOrWhiteSpace(executionToken) ||
            !System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(
                input: authorization, parsedValue: out var header) ||
            !string.Equals(a: header.Scheme, b: "Bearer", comparisonType: StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(header.Parameter))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            left: Encoding.UTF8.GetBytes(s: header.Parameter),
            right: Encoding.UTF8.GetBytes(s: executionToken));
    }

    internal static bool IsTrustedApiRoot(string apiRoot, string trustedOrigins)
    {
        if (string.IsNullOrWhiteSpace(value: trustedOrigins) ||
            !Uri.TryCreate(uriString: apiRoot, uriKind: UriKind.Absolute, result: out Uri api) ||
            api.Scheme != Uri.UriSchemeHttps || api.UserInfo.Length != 0 ||
            api.Query.Length != 0 || api.Fragment.Length != 0 ||
            !api.AbsolutePath.Equals(value: "/Api/", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return trustedOrigins
            .Split(separator: ';', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(predicate: entry =>
                Uri.TryCreate(uriString: entry, uriKind: UriKind.Absolute, result: out Uri origin) &&
                origin.Scheme == Uri.UriSchemeHttps && origin.UserInfo.Length == 0 &&
                origin.AbsolutePath == "/" && origin.Query.Length == 0 && origin.Fragment.Length == 0 &&
                origin.Port == api.Port &&
                origin.IdnHost.Equals(value: api.IdnHost, comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<IResult> ExecuteAsync(
        HttpRequest request,
        IFlowRunner flowRunner,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        RequireContentType(
            request: request,
            expectedMediaType: "application/json");

        string json = await ReadBodyAsync(
            request: request,
            cancellationToken: cancellationToken);

        WorkflowRequest workflowRequest;

        try
        {
            workflowRequest =
                JsonConvert.DeserializeObject<WorkflowRequest>(
                    value: json,
                    settings: GetJsonSettings())
                ?? throw new BadHttpRequestException(
                    message:
                        "Workflow request payload could not be deserialized.");
        }
        catch (JsonException exception)
        {
            throw new BadHttpRequestException(
                message: "Workflow request payload is not valid JSON.",
                innerException: exception);
        }

        if (string.IsNullOrWhiteSpace(value: workflowRequest.AuthToken))
        {
            return Results.Unauthorized();
        }

        if (!IsTrustedApiRoot(
            apiRoot: workflowRequest.Api,
            trustedOrigins: configuration["Workflow:TrustedWebOrigins"]))
        {
            return Results.StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }

        await flowRunner.RunAsync(workflowRequest: workflowRequest);

        return Results.Text(content: "OK");
    }

    private static async Task<IResult> ExecuteScriptAsync(
        HttpRequest request,
        bool useDetails,
        IWorkflowScriptExecutionService scriptExecutionService,
        CancellationToken cancellationToken)
    {
        RequireContentType(
            request: request,
            expectedMediaType: "text/plain");

        string payload = await ReadBodyAsync(
            request: request,
            cancellationToken: cancellationToken);

        string result = await scriptExecutionService.ExecuteAsync(
            payload: payload,
            useDetails: useDetails);

        return Results.Text(content: result);
    }

    private static async Task<string> ReadBodyAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength > MaximumRequestSize)
        {
            throw CreatePayloadTooLargeException();
        }

        using StreamReader reader = new(
            stream: request.Body,
            encoding: System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 8192,
            leaveOpen: true);

        char[] buffer = new char[8192];
        int totalCharacters = 0;
        System.Text.StringBuilder body = new();

        while (true)
        {
            int charactersRead = await reader.ReadAsync(
                buffer: buffer,
                cancellationToken: cancellationToken);

            if (charactersRead == 0)
            {
                break;
            }

            totalCharacters += charactersRead;

            if (totalCharacters > MaximumRequestSize)
            {
                throw CreatePayloadTooLargeException();
            }

            body.Append(
                value: buffer.AsSpan(
                    start: 0,
                    length: charactersRead));
        }

        return body.ToString();
    }

    private static BadHttpRequestException CreatePayloadTooLargeException() =>
        new(
            message: "The request body exceeds the one megabyte limit.",
            statusCode: StatusCodes.Status413PayloadTooLarge);

    private static JsonSerializerSettings GetJsonSettings() =>
        new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = true,
            },
        };

    private static void RequireContentType(
        HttpRequest request,
        string expectedMediaType)
    {
        if (!request.ContentType?.StartsWith(
            value: expectedMediaType,
            comparisonType: StringComparison.OrdinalIgnoreCase)
            ?? true)
        {
            throw new BadHttpRequestException(
                message:
                    $"Content-Type must be '{expectedMediaType}'.",
                statusCode:
                    StatusCodes.Status415UnsupportedMediaType);
        }
    }
}