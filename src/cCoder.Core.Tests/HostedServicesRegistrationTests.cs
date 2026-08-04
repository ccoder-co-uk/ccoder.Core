// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Models;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class HostedServicesRegistrationTests
{
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