// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class SampleAppConfigurationTests
{
    private static readonly string[] RequiredConfigurationKeys =
    [
        "AI:DefaultProvider",
        "AI:Agent:MaxIterations",
        "AI:Agent:ShellCommandTimeoutSeconds",
        "AI:Agent:StreamingChunkCharacterCount",
        "AI:Agent:StreamingChunkDelayMilliseconds",
        "AI:Providers:Ollama:Name",
        "AI:Providers:Ollama:CompletionProvider:Mode",
        "AI:Providers:Ollama:CompletionProvider:Endpoint",
        "AI:Providers:Ollama:CompletionProvider:DefaultModel",
        "AI:Providers:Ollama:CompletionProvider:ApiKey",
        "AI:Providers:Ollama:CompletionProvider:TimeoutSeconds",
        "AI:Providers:Ollama:CompletionProvider:Temperature",
        "AI:Providers:Ollama:ModelProvider:Mode",
        "AI:Providers:Ollama:ModelProvider:Endpoint",
        "AI:Providers:Ollama:ModelProvider:ApiKey",
        "AI:Providers:Ollama:ModelProvider:TimeoutSeconds",
        "Logging:ConnectionString",
        "Logging:DebugInfo",
        "Logging:LogSQL",
        "Logging:StoreLogEntries",
        "Logging:StreamLogEntries",
        "Logging:RetentionDays",
        "Logging:RetentionIntervalMinutes",
        "Logging:DefaultAppId",
        "Logging:DefaultAppDomain",
        "Logging:RootPath",
        "AppSecurity:ConnectionString",
        "AppSecurity:AggregateDomains",
        "AppSecurity:DebugInfo",
        "AppSecurity:LogSQL",
        "AppSecurity:RootPath",
        "AppSecurity:IncludeLegacyCoreContext",
        "AppSecurity:IsMigrating",
        "Security:ConnectionString",
        "Security:DecryptionKey",
        "Security:RootPath",
        "Security:IsMigrating",
        "Security:MaxFailedAccessAttempts",
        "Security:LockoutDurationMinutes",
        "Security:Argon:MemorySizeInKilobytes",
        "Security:Argon:Iterations",
        "Security:Argon:DegreeOfParallelism",
        "Security:Argon:SaltSizeInBytes",
        "Security:Argon:HashSizeInBytes",
        "ContentManagement:ConnectionString",
        "ContentManagement:CacheSource",
        "ContentManagement:CacheSourceAppId",
        "ContentManagement:CacheExpiry",
        "ContentManagement:WorkflowServiceUrl",
        "ContentManagement:DebugInfo",
        "ContentManagement:LogSQL",
        "ContentManagement:RootPath",
        "ContentManagement:IncludeLegacyCoreContext",
        "DocumentManagement:ConnectionString",
        "DocumentManagement:DebugInfo",
        "DocumentManagement:LogSQL",
        "DocumentManagement:RootPath",
        "Mail:ConnectionString",
        "Mail:DebugInfo",
        "Mail:LogSQL",
        "Mail:RootPath",
        "Mail:IsMigrating",
        "Packaging:ConnectionString",
        "Packaging:AssetsRoot",
        "Packaging:PackageSourceSslPort",
        "Packaging:RootPath",
        "Workflow:ConnectionString",
        "Workflow:ServiceUrl",
        "Workflow:SslPort",
        "Workflow:InstanceMaintenance:MaxAgeDays",
        "Workflow:QueueInstanceManagement:ExecutingTimeoutMinutes",
        "Workflow:QueueInstanceManagement:PollingIntervalMilliseconds",
        "Workflow:DebugInfo",
        "Workflow:LogSQL",
        "Workflow:RootPath",
        "Workflow:IncludeLegacyCoreContext",
        "Workflow:IsMigrating",
        "Eventing:ProviderType",
        "Eventing:Http:HubUrl",
        "Eventing:Http:MaxConcurrency",
        "Eventing:ServiceBus:ConnectionString",
        "Eventing:ServiceBus:MaxConcurrency",
        "Api:ExposeDocumentation",
        "Api:ExposeMetadata"
    ];

    [Theory]
    [InlineData("Web.appsettings.json")]
    [InlineData("HostedServices.appsettings.json")]
    public void SuppliedApp_ShouldDeclareRequiredConfiguration(
        string fileName)
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(basePath: AppContext.BaseDirectory)
            .AddJsonFile(
                path: Path.Combine(
                    path1: "Configuration",
                    path2: fileName),
                optional: false)
            .Build();

        // When
        string[] missingKeys = RequiredConfigurationKeys
            .Where(predicate: key => configuration[key] is null)
            .ToArray();

        // Then
        missingKeys
            .Should()
            .BeEmpty();

        string assetsRoot = configuration["Packaging:AssetsRoot"]!;

        if (string.Equals(
            a: fileName,
            b: "Web.appsettings.json",
            comparisonType: StringComparison.Ordinal))
        {
            assetsRoot
                .Should()
                .Be(expected: "/Assets/");
        }
        else
        {
            Uri.TryCreate(
                    uriString: assetsRoot,
                    uriKind: UriKind.Absolute,
                    result: out Uri assetsRootUri)
                .Should()
                .BeTrue();

            assetsRootUri.Scheme
                .Should()
                .Be(expected: Uri.UriSchemeHttps);
        }

        Uri.TryCreate(
                uriString: configuration["Eventing:Http:HubUrl"],
                uriKind: UriKind.Absolute,
                result: out Uri eventHubUrl)
            .Should()
            .BeTrue();

        eventHubUrl.Scheme
            .Should()
            .Be(expected: Uri.UriSchemeHttps);
    }

    [Fact]
    public void SuppliedWorkflowApp_ShouldDeclareRequiredConfiguration()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(basePath: AppContext.BaseDirectory)
            .AddJsonFile(
                path: Path.Combine(
                    path1: "Configuration",
                    path2: "Workflow.local.settings.json"),
                optional: false)
            .Build();

        // When
        string[] missingKeys =
        [
            "Values:Data__ConnectionString",
            "Values:Data__DebugInfo",
            "Values:Data__LogSQL"
        ];

        missingKeys = missingKeys
            .Where(predicate: key => configuration[key] is null)
            .ToArray();

        // Then
        missingKeys
            .Should()
            .BeEmpty();
    }
}