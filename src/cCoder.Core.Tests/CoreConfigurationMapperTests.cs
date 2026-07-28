// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using FluentAssertions;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CoreConfigurationMapperTests
{
    [Fact]
    public void ApplyBoundRootSections_ShouldPreserveExplicitHttpEventing()
    {
        // Given
        CoreConfiguration configuration = new()
        {
            EnableHttpEventing = true,
            EventProviderType = "Http",
            HttpEventHubUrl = string.Empty,
        };

        // When
        CoreConfigurationMapper.ApplyBoundRootSections(target: configuration);

        // Then
        configuration.EnableHttpEventing
            .Should()
            .BeTrue();
    }

    [Fact]
    public void ApplyBoundRootSections_ShouldPreserveExplicitServiceBusEventing()
    {
        // Given
        CoreConfiguration configuration = new()
        {
            EnableServiceBusEventing = true,
            EventProviderType = "ServiceBus",
            ServiceBusConnectionString = string.Empty,
        };

        // When
        CoreConfigurationMapper.ApplyBoundRootSections(target: configuration);

        // Then
        configuration.EnableServiceBusEventing
            .Should()
            .BeTrue();
    }

    [Fact]
    public void ApplyDomainDefaults_ShouldInheritRootValuesAndPreserveDomainOverrides()
    {
        // Given
        CoreConfiguration configuration = new()
        {
            ConnectionStrings = new Dictionary<string, string> { ["Core"] = "root-database" },
            Settings = new Dictionary<string, string> { ["Shared"] = "root-setting" },
            Services = new Dictionary<string, string> { ["Workflow"] = "root-service" },
            DebugInfo = true,
            LogSQL = true,
        };

        configuration.ContentManagement.Settings["Shared"] = "domain-setting";

        // When
        CoreConfigurationMapper.ApplyDomainDefaults(target: configuration);

        // Then
        configuration.AppSecurity.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.ContentManagement.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.DocumentManagement.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.DomainLogging.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.Mail.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.Workflow.ConnectionStrings["Core"]
            .Should()
            .Be(expected: "root-database");

        configuration.ContentManagement.Settings["Shared"]
            .Should()
            .Be(expected: "domain-setting");

        configuration.Workflow.Services["Workflow"]
            .Should()
            .Be(expected: "root-service");

        configuration.AppSecurity.DebugInfo
            .Should()
            .BeTrue();

        configuration.Mail.LogSQL
            .Should()
            .BeTrue();

    }
}