// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
using cCoder.Core.Models;
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
                ["ContentManagement:RootPath"] = "Api/BoundContent"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);
        CoreConfiguration coreConfiguration = new();
        configuration.Bind(coreConfiguration);

        // When
        services.AddCoreHostedServices(coreConfiguration);

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
                ["ContentManagement:WorkflowServiceUrl"] = "http://localhost:7071/api/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);
        CoreConfiguration coreConfiguration = new();
        configuration.Bind(coreConfiguration);
        coreConfiguration.Eventing.ProviderType = "ServiceBus";
        coreConfiguration.Eventing.ServiceBus.MaxConcurrency = 3;

        // When
        services.AddCoreHostedServices(coreConfiguration);

        // Then
        services.Should()
            .Contain(predicate: descriptor =>
            descriptor.ServiceType == typeof(IAzureServiceBusEventHub));

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