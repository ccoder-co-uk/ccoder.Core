// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.DocumentManagement.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Models;
using cCoder.Workflow.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CoreConfigurationBindingTests
{
    [Fact]
    public void BusinessDomainConfigurations_ShouldNotOwnPersistenceSettings()
    {
        // Given
        Type[] businessDomainConfigurationTypes =
        [
            typeof(AppSecurityConfiguration),
            typeof(ContentManagementConfiguration),
            typeof(DocumentManagementConfiguration),
            typeof(LoggingConfiguration),
            typeof(MailConfiguration),
            typeof(PackagingConfiguration),
            typeof(SecurityConfiguration),
            typeof(WorkflowConfiguration)
        ];

        // When
        string[] persistenceProperties = businessDomainConfigurationTypes
            .SelectMany(selector: type => type.GetProperties())
            .Where(predicate: property =>
                property.Name is "ConnectionString" or "DebugInfo" or "LogSQL")
            .Select(selector: property =>
                $"{property.DeclaringType!.Name}.{property.Name}")
            .ToArray();

        // Then
        persistenceProperties
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void CoreConfiguration_ShouldSupportExtension()
    {
        // Given
        Type configurationType = typeof(CoreConfiguration);

        // When
        bool isSealed = configurationType.IsSealed;

        // Then
        isSealed
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Bind_ShouldPopulateDerivedApplicationConfiguration()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["CoreData:ConnectionString"] = "core",
                ["ApplicationDomain:Value"] = "extension"
            })
            .Build();

        // When
        ExtendedCoreConfiguration result =
            CoreConfigurationFactory.Create<ExtendedCoreConfiguration>(
                configuration: configuration);

        // Then
        result.CoreData.ConnectionString
            .Should()
            .Be(expected: "core");

        result.ApplicationDomain.Value
            .Should()
            .Be(expected: "extension");
    }

    [Fact]
    public void CoreConfiguration_ShouldFailForMalformedAiSection()
    {
        // Given
        IConfiguration applicationConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["AI:Agent:MaxIterations"] =
                    "NotAnInteger"
            })
            .Build();

        // When
        Action action = () =>
            _ = CoreConfigurationFactory.Create(
                configuration: applicationConfiguration);

        // Then
        action
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void CoreConfiguration_ShouldOnlyExposeDomainConfigurationObjects()
    {
        // Given
        Type configurationType = typeof(CoreConfiguration);

        // When
        System.Reflection.PropertyInfo[] properties =
            configurationType.GetProperties();

        // Then
        properties
            .Should()
            .OnlyContain(
                predicate: property =>
                    property.PropertyType.IsClass
                    || Attribute.IsDefined(
                        element: property,
                        attributeType: typeof(JsonIgnoreAttribute)));
    }

    [Fact]
    public void Bind_ShouldCreateStructuredDomainConfigurations()
    {
        // Given
        Dictionary<string, string> values = new()
        {
            ["CoreData:ConnectionString"] = "core",
            ["CoreData:AdminConnectionString"] = "core-admin",
            ["AppSecurity:AggregateDomains"] = "true",
            ["SecurityData:ConnectionString"] = "sso",
            ["SecurityData:AdminConnectionString"] = "sso-admin",
            ["Security:DecryptionKey"] = "key",
            ["ContentManagement:RootPath"] = "Api/Content",
            ["Mail:Providers:MicrosoftGraph:TenantId"] = "tenant",
            ["Eventing:ProviderType"] = "ServiceBus",
            ["Eventing:ServiceBus:ConnectionString"] = "events",
            ["Eventing:ServiceBus:MaxConcurrency"] = "8",
            ["Api:ExposeDocumentation"] = "true",
            ["Api:ExposeMetadata"] = "false",
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: values)
            .Build();

        // When
        CoreConfiguration result =
            CoreConfigurationFactory.Create(
                configuration: configuration);

        // Then
        result.CoreData.ConnectionString
            .Should()
            .Be(expected: "core");

        result.CoreData.AdminConnectionString
            .Should()
            .Be(expected: "core-admin");

        result.SecurityData.ConnectionString
            .Should()
            .Be(expected: "sso");

        result.SecurityData.AdminConnectionString
            .Should()
            .Be(expected: "sso-admin");

        result.Security.DecryptionKey
            .Should()
            .Be(expected: "key");

        result.ContentManagement.RootPath
            .Should()
            .Be(expected: "Api/Content");

        result.Mail.Providers["MicrosoftGraph"].TenantId
            .Should()
            .Be(expected: "tenant");

        result.Eventing.ProviderType
            .Should()
            .Be(expected: "ServiceBus");

        result.Eventing.ServiceBus.ConnectionString
            .Should()
            .Be(expected: "events");

        result.Eventing.ServiceBus.MaxConcurrency
            .Should()
            .Be(expected: 8);

        result.Eventing.EventProviders
            .Should()
            .BeEmpty();

        result.Api.ExposeDocumentation
            .Should()
            .BeTrue();

        result.Api.ExposeMetadata
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Bind_ShouldNotProjectLegacyConnectionsIntoDataDomains()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["ContentManagement:ConnectionString"] = "legacy-core",
                ["Security:ConnectionString"] = "legacy-sso"
            })
            .Build();

        // When
        CoreConfiguration result = CoreConfigurationFactory.Create(
            configuration: configuration);

        // Then
        result.CoreData
            .Should()
            .BeNull();

        result.SecurityData
            .Should()
            .BeNull();
    }

    [Fact]
    public void FromConfiguration_ShouldOmitUnconfiguredDomains()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["Api:ExposeDocumentation"] = "true"
            })
            .Build();

        // When
        CoreConfiguration result =
            CoreConfigurationFactory.Create(
                configuration: configuration);

        // Then
        object[] domains =
        [
            result.AI,
            result.CoreData,
            result.AppSecurity,
            result.ContentManagement,
            result.DocumentManagement,
            result.Logging,
            result.Mail,
            result.Packaging,
            result.Security,
            result.SecurityData,
            result.Workflow
        ];

        domains
            .Should()
            .OnlyContain(predicate: domain => domain == null);

        result.Api.ExposeDocumentation
            .Should()
            .BeTrue();
    }

    [Fact]
    public void FromConfiguration_GivenInvalidConfiguredDomain_ShouldFailClearly()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["Workflow:SslPort"] = "not-a-port"
            })
            .Build();

        // When
        Action action = () =>
            _ = CoreConfigurationFactory.Create(configuration: configuration);

        // Then
        InvalidOperationException exception = action
            .Should()
            .Throw<InvalidOperationException>()
            .Which;

        exception.Message
            .Should()
            .Contain(expected: "Workflow:SslPort");
    }

    private sealed class ExtendedCoreConfiguration : CoreConfiguration
    {
        public ApplicationDomainConfiguration ApplicationDomain { get; set; }
    }

    private sealed class ApplicationDomainConfiguration
    {
        public string Value { get; set; }
    }
}