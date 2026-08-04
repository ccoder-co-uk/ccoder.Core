// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CoreConfigurationBindingTests
{
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
            _ = new CoreConfiguration(applicationConfiguration);

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
            ["AppSecurity:ConnectionString"] = "app-security",
            ["Security:ConnectionString"] = "sso",
            ["Security:DecryptionKey"] = "key",
            ["ContentManagement:ConnectionString"] = "content",
            ["ContentManagement:RootPath"] = "Api/Content",
            ["CRM:ConnectionString"] = "crm",
            ["CRM:AdminConnectionString"] = "crm-admin",
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
            new(configuration);

        // Then
        result.AppSecurity.ConnectionString
            .Should()
            .Be(expected: "app-security");

        result.Security.ConnectionString
            .Should()
            .Be(expected: "sso");

        result.Security.DecryptionKey
            .Should()
            .Be(expected: "key");

        result.ContentManagement.ConnectionString
            .Should()
            .Be(expected: "content");

        result.ContentManagement.RootPath
            .Should()
            .Be(expected: "Api/Content");

        result.CRM.ConnectionString
            .Should()
            .Be(expected: "crm");

        result.CRM.AdminConnectionString
            .Should()
            .Be(expected: "crm-admin");

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
            new(configuration);

        // Then
        object[] domains =
        [
            result.AI,
            result.AppSecurity,
            result.ContentManagement,
            result.CRM,
            result.DocumentManagement,
            result.Logging,
            result.Mail,
            result.Packaging,
            result.Security,
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
            _ = new CoreConfiguration(configuration: configuration);

        // Then
        InvalidOperationException exception = action
            .Should()
            .Throw<InvalidOperationException>()
            .Which;

        exception.Message
            .Should()
            .Contain(expected: "Workflow:SslPort");
    }
}