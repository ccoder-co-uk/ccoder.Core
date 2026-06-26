using cCoder.Core;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.AzureServiceBus.Models;
using cCoder.Scheduling.Exposures.HostedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace cCoder.Core.Tests;

public sealed class HostedServicesRegistrationTests
{
    [Fact]
    public void AddCoreHostedServices_ShouldRegisterTaskRunnerHostedService()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:Core"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:SSO"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["Settings:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["Services:Workflow"] = "http://localhost:7071/api/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(configuration);

        services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
            {
                coreConfig.CoreConnectionString = configuration["ConnectionStrings:Core"];
                coreConfig.SecurityConnectionString = configuration["ConnectionStrings:SSO"];
                coreConfig.DecryptionKey = configuration["Settings:DecryptionKey"];
                coreConfig.WorkflowServiceUrl = configuration["Services:Workflow"];
            });
        });

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType == typeof(TaskRunnerHostedService));
    }

    [Fact]
    public void AddCoreHostedServices_GivenServiceBusEventing_ShouldRegisterServiceBusHubWithConcurrency()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:Core"] = "Server=(localdb)\\mssqllocaldb;Database=core-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:SSO"] = "Server=(localdb)\\mssqllocaldb;Database=sso-tests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["ConnectionStrings:ServiceBus"] = "Endpoint=sb://acceptance.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123=",
                ["Settings:DecryptionKey"] = "000000000000000000000000000000000000000000000000",
                ["Services:Workflow"] = "http://localhost:7071/api/"
            })
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(configuration);

        services.AddCoreHostedServices(coreBuilder =>
        {
            coreBuilder.ConfigureDomainsWith(coreConfig =>
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

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IAzureServiceBusEventHub));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        AzureServiceBusEventingConfiguration eventingConfiguration =
            serviceProvider.GetRequiredService<AzureServiceBusEventingConfiguration>();

        eventingConfiguration.ConnectionString.Should().Be(configuration["ConnectionStrings:ServiceBus"]);
        eventingConfiguration.MaxConcurrency.Should().Be(3);
    }
}
