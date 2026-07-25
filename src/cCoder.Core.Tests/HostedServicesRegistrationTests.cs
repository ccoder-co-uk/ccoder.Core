// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;
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
                ["ConnectionStrings:Core"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:SSO"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Settings:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["Services:Workflow"] = "http://localhost:7071/api/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);

        // When
        services.AddCoreHostedServices(configure: coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(configure: coreConfig =>
            {
                coreConfig.CoreConnectionString = configuration["ConnectionStrings:Core"];
                coreConfig.SecurityConnectionString = configuration["ConnectionStrings:SSO"];
                coreConfig.DecryptionKey = configuration["Settings:DecryptionKey"];
                coreConfig.WorkflowServiceUrl = configuration["Services:Workflow"];
            });
        });

        // Then
        services.Count(predicate: descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationFactory is not null)
            .Should()
            .BeGreaterThanOrEqualTo(expected: 3);

        services.Should()
            .NotContain(predicate: descriptor =>
                descriptor.ServiceType == typeof(IAzureServiceBusEventHub));
    }

    [Fact]
    public void AddCoreHostedServices_GivenServiceBusEventing_ShouldRegisterServiceBusHubWithConcurrency()
    {
        // Given
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["ConnectionStrings:Core"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:SSO"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:ServiceBus"] = "Endpoint=sb://acceptance.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123=",
                ["Settings:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["Services:Workflow"] = "http://localhost:7071/api/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(implementationInstance: configuration);

        // When
        services.AddCoreHostedServices(configure: coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(configure: coreConfig =>
            {
                coreConfig.CoreConnectionString = configuration["ConnectionStrings:Core"];
                coreConfig.SecurityConnectionString = configuration["ConnectionStrings:SSO"];
                coreConfig.ServiceBusConnectionString = configuration["ConnectionStrings:ServiceBus"];
                coreConfig.DecryptionKey = configuration["Settings:DecryptionKey"];
                coreConfig.WorkflowServiceUrl = configuration["Services:Workflow"];
                coreConfig.EventProviderType = "ServiceBus";
                coreConfig.EnableServiceBusEventing = true;
                coreConfig.MaxConcurrency = 3;
            });
        });

        // Then
        services.Should()
            .Contain(predicate: descriptor =>
            descriptor.ServiceType == typeof(IAzureServiceBusEventHub));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AzureServiceBusEventingConfiguration eventingConfiguration =
            serviceProvider.GetRequiredService<AzureServiceBusEventingConfiguration>();

        eventingConfiguration.ConnectionString.Should()
            .Be(expected: configuration["ConnectionStrings:ServiceBus"]);

        eventingConfiguration.MaxConcurrency.Should()
            .Be(expected: 3);
    }
}