// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Eventing.Sql;
using cCoder.Eventing.Sql.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class HostedServicesRegistrationTests
{
    [Fact]
    public void AddCoreHostedServices_GivenSqlEventing_RegistersSqlWorkerAndHub()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Eventing.ProviderType = "Sql";
        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreHostedServices(configuration: configuration);

        // Then
        services
            .Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType == typeof(ISqlEventHub));

        services
            .Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType == typeof(ISqlEventFailureHub));

        services
            .Should()
            .Contain(predicate: descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType != null
            && descriptor.ImplementationType.Name == "SqlEventBackgroundService");
    }

    [Fact]
    public void AddCoreHostedServices_GivenSqlEventing_BindsWorkerConcurrency()
    {
        // Given
        IConfiguration settings = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["Eventing:Sql:MaxConcurrency"] = "7",
                ["Eventing:Sql:PollInterval"] = "00:01:00",
                ["Eventing:Sql:LeaseDuration"] = "00:15:00"
            })
            .Build();

        CoreConfiguration configuration = CoreConfigurationFactory.Create(
            configuration: settings);

        configuration.Eventing.ProviderType = "Sql";
        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreHostedServices(configuration: configuration);

        // Then
        using ServiceProvider provider = services.BuildServiceProvider();
        SqlEventingOptions options = provider.GetRequiredService<SqlEventingOptions>();

        options.MaxConcurrency
            .Should()
            .Be(expected: 7);

        options.PollInterval
            .Should()
            .Be(expected: TimeSpan.FromMinutes(value: 1));

        options.LeaseDuration
            .Should()
            .Be(expected: TimeSpan.FromMinutes(value: 15));
    }

    [Fact]
    public void AddCoreHostedServices_ShouldRegisterWorkflowHostedServices()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["AppSecurity:ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Security:ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Security:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["ContentManagement:WorkflowServiceUrl"] = "http://localhost:7071/api/",
                ["ContentManagement:RootPath"] = "Api/BoundContent",
                ["Workflow:ServiceUrl"] = "https://localhost:7100/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);

        CoreConfiguration coreConfiguration =
            CoreConfigurationFactory.Create(
                configuration: configuration);

        // When
        services.AddCoreHostedServices(
            configuration: coreConfiguration);

        // Then
        services.Count(predicate: descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationFactory is not null)
            .Should()
            .BeGreaterThanOrEqualTo(expected: 3);

        services.Should()
            .NotContain(predicate: descriptor =>
                descriptor.ServiceType == typeof(IAzureServiceBusEventHub));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        serviceProvider
            .GetRequiredService<cCoder.ContentManagement.Models.ContentManagementConfiguration>()
            .RootPath
            .Should()
            .Be(expected: "Api/BoundContent");
    }

    [Fact]
    public void AddCoreHostedServices_GivenServiceBusEventing_ShouldRegisterServiceBusHubWithConcurrency()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["AppSecurity:ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Security:ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Eventing:ServiceBus:ConnectionString"] = "Endpoint=sb://acceptance.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123=",
                ["Security:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["ContentManagement:WorkflowServiceUrl"] = "http://localhost:7071/api/",
                ["Workflow:ServiceUrl"] = "https://localhost:7100/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);

        CoreConfiguration coreConfiguration =
            CoreConfigurationFactory.Create(
                configuration: configuration);

        coreConfiguration.Eventing.ProviderType = "ServiceBus";
        coreConfiguration.Eventing.ServiceBus.MaxConcurrency = 3;

        // When
        services.AddCoreHostedServices(
            configuration: coreConfiguration);

        // Then
        services.Should()
            .Contain(predicate: descriptor =>
            descriptor.ServiceType == typeof(IAzureServiceBusEventHub));

        services.Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType ==
                    typeof(IServiceBusAppDeleteForwardingBroker));

        services.Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType ==
                    typeof(IServiceBusFolderDeleteForwardingBroker));

        services.Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType ==
                    typeof(ServiceBusAppDeleteForwardingService));

        services.Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType ==
                    typeof(ServiceBusFolderDeleteForwardingService));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AzureServiceBusEventingConfiguration eventingConfiguration =
            serviceProvider.GetRequiredService<AzureServiceBusEventingConfiguration>();

        eventingConfiguration.ConnectionString.Should()
            .Be(expected: configuration[
                "Eventing:ServiceBus:ConnectionString"]);

        eventingConfiguration.MaxConcurrency.Should()
            .Be(expected: 3);
    }
}