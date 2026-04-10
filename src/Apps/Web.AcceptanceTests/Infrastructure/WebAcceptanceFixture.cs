using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Web.AcceptanceTests.Models;
using Xunit;


namespace Web.AcceptanceTests.Infrastructure;

public sealed class WebAcceptanceFixture : IAsyncLifetime
{
    private AcceptanceDatabaseManager databaseManager;

    internal WebAcceptanceFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AcceptanceSettings settings = new()
        {
            CoreConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_CORE_CONNECTION_STRING"),
            SsoConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_SSO_CONNECTION_STRING"),
            DecryptionKey = "000000000000000000000000000000000000000000000000",
        };

        Factory = new WebAcceptanceFactory(settings);
        databaseManager = new AcceptanceDatabaseManager(Factory.Services);
        await databaseManager.ResetDatabasesAsync();
        await SeedAsync();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (databaseManager is not null)
            await databaseManager.DropDatabasesAsync();

        if (Factory is not null)
            await Factory.DisposeAsync();
    }

    private Task SeedAsync() =>
        new AcceptanceApplicationSeeder(Factory.Services).SeedAsync();

    private static string AddDatabaseSuffix(string variableName)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
            return string.Empty;

        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true,
        };
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName))
            return connectionString;

        string suffix = typeof(WebAcceptanceFixture).Assembly.GetName().Name!
            .Replace(".AcceptanceTests", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        builder.InitialCatalog = $"{databaseName}-{suffix}";
        return builder.ConnectionString;
    }
}

[CollectionDefinition(Name)]
public sealed class WebAcceptanceCollection : ICollectionFixture<WebAcceptanceFixture>
{
    public const string Name = "Web acceptance";
}

