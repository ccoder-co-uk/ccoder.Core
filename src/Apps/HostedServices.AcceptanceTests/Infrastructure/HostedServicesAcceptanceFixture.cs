using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using HostedServices.AcceptanceTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HostedServices.AcceptanceTests.Infrastructure;

public sealed class HostedServicesAcceptanceFixture : IAsyncLifetime
{
    private AcceptanceDatabaseManager databaseManager;
    private ServiceProvider databaseServices;
    private Dictionary<string, string> previousEnvironmentValues;
    internal HostedServicesAcceptanceFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AcceptanceSettings settings = new()
        {
            CoreConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_CORE_CONNECTION_STRING"),
            SsoConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_SSO_CONNECTION_STRING"),
            DecryptionKey = "000000000000000000000000000000000000000000000000",
        };

        ApplyEnvironment(settings);
        databaseServices = AcceptanceServiceProviderFactory.Create(settings);
        Factory = new HostedServicesAcceptanceFactory(settings);
        databaseManager = new AcceptanceDatabaseManager(databaseServices);
        await databaseManager.ResetDatabasesAsync();
        await new AcceptanceApplicationSeeder(Factory.Services).SeedAsync();
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

        if (databaseServices is not null)
            await databaseServices.DisposeAsync();

        if (Factory is not null)
            await Factory.DisposeAsync();

        RestoreEnvironment();
    }

    private void ApplyEnvironment(AcceptanceSettings settings)
    {
        previousEnvironmentValues = new Dictionary<string, string>
        {
            ["ConnectionStrings__Core"] = Environment.GetEnvironmentVariable("ConnectionStrings__Core"),
            ["ConnectionStrings__SSO"] = Environment.GetEnvironmentVariable("ConnectionStrings__SSO"),
            ["Settings__DecryptionKey"] = Environment.GetEnvironmentVariable("Settings__DecryptionKey"),
            ["Eventing__Http__HubUrl"] = Environment.GetEnvironmentVariable("Eventing__Http__HubUrl"),
        };

        Environment.SetEnvironmentVariable("ConnectionStrings__Core", settings.CoreConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__SSO", settings.SsoConnectionString);
        Environment.SetEnvironmentVariable("Settings__DecryptionKey", settings.DecryptionKey);
        Environment.SetEnvironmentVariable("Eventing__Http__HubUrl", string.Empty);
    }

    private void RestoreEnvironment()
    {
        if (previousEnvironmentValues is null)
            return;

        foreach ((string name, string value) in previousEnvironmentValues)
            Environment.SetEnvironmentVariable(name, value);
    }

    private static string AddDatabaseSuffix(string variableName)
    {
        string connectionString = ReadRequiredConnectionString(variableName);

        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true,
        };
        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName))
            return connectionString;

        string suffix = typeof(HostedServicesAcceptanceFixture).Assembly.GetName().Name!
            .Replace(".AcceptanceTests", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        builder.InitialCatalog = $"{databaseName}-{suffix}";
        return builder.ConnectionString;
    }

    private static string ReadRequiredConnectionString(string variableName)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(variableName)
            ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        throw new InvalidOperationException(
            $"Acceptance connection string environment variable '{variableName}' was not found.");
    }
}

[CollectionDefinition(Name)]
public sealed class HostedServicesAcceptanceCollection
    : ICollectionFixture<HostedServicesAcceptanceFixture>
{
    public const string Name = "HostedServices acceptance";
}
