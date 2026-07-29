// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;
using cCoder.Core.Testing;
using Web.AcceptanceTests.Models;
using Xunit;


namespace Web.AcceptanceTests.Infrastructure;

public sealed class WebAcceptanceFixture : IAsyncLifetime
{
    private AcceptanceDatabaseManager databaseManager;

    internal WebAcceptanceFactory Factory { get; private set; } = null!;

    internal AcceptanceSettings Settings { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AcceptanceTestConfiguration configuration =
            AcceptanceTestConfiguration.Load();

        Settings = new()
        {
            CoreConnectionString = configuration.CoreConnectionString,
            SsoConnectionString = configuration.SecurityConnectionString,
            DecryptionKey = configuration.DecryptionKey,
        };

        Factory = new WebAcceptanceFactory(Settings);

        databaseManager = new AcceptanceDatabaseManager(
            Factory.Services,
            Settings.CoreConnectionString,
            Settings.SsoConnectionString);

        await databaseManager.ResetDatabasesAsync();
        await SeedAsync();

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
        }
        finally
        {
            if (databaseManager is not null)
            {
                await databaseManager.DropDatabasesAsync();
            }
        }
    }

    private Task SeedAsync() =>
        new AcceptanceApplicationSeeder(Factory.Services).SeedAsync();

}

[CollectionDefinition(Name)]
public sealed class WebAcceptanceCollection : ICollectionFixture<WebAcceptanceFixture>
{
    public const string Name = "Web acceptance";
}