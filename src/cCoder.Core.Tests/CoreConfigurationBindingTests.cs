// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CoreConfigurationBindingTests
{
    [Fact]
    public void CoreConfiguration_ShouldOnlyExposeDomainConfigurationObjects()
    {
        typeof(CoreConfiguration)
            .GetProperties()
            .Should()
            .OnlyContain(property => property.PropertyType.IsClass);
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
            ["Mail:MicrosoftGraph:TenantId"] = "tenant",
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
        CoreConfiguration result = configuration.Get<CoreConfiguration>();

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

        result.Mail.MicrosoftGraph.TenantId
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
}
