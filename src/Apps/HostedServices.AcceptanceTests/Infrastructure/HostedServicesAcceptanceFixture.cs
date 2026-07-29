// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;
using cCoder.Core.Testing;
using HostedServices.AcceptanceTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HostedServices.AcceptanceTests.Infrastructure;

public sealed class HostedServicesAcceptanceFixture : IAsyncLifetime
{
    private AcceptanceDatabaseManager databaseManager;
    private ServiceProvider databaseServices;
    internal HostedServicesAcceptanceFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AcceptanceTestConfiguration configuration =
            AcceptanceTestConfiguration.Load();

        AcceptanceSettings settings = new()
        {
            CoreConnectionString = configuration.CoreConnectionString,
            SsoConnectionString = configuration.SecurityConnectionString,
            DecryptionKey = configuration.DecryptionKey,
        };

        databaseServices = AcceptanceServiceProviderFactory.Create(settings: settings);
        Factory = new HostedServicesAcceptanceFactory(settings);

        databaseManager = new AcceptanceDatabaseManager(
            databaseServices,
            settings.CoreConnectionString,
            settings.SsoConnectionString);

        await databaseManager.ResetDatabasesAsync();
        await new AcceptanceApplicationSeeder(Factory.Services).SeedAsync();

        Client = Factory.CreateClient(options: new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        try
        {
            if (Factory is not null)
            {
                await Factory.DisposeAsync();
            }

            if (databaseServices is not null)
            {
                await databaseServices.DisposeAsync();
            }
        }
        finally
        {
            if (databaseManager is not null)
            {
                await databaseManager.DropDatabasesAsync();
            }
        }
    }
}

[CollectionDefinition(Name)]
public sealed class HostedServicesAcceptanceCollection
    : ICollectionFixture<HostedServicesAcceptanceFixture>
{
    public const string Name = "HostedServices acceptance";
}