// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
            CoreConnectionString = AddDatabaseSuffix(variableName: "ConnectionStrings__Core"),
            SsoConnectionString = AddDatabaseSuffix(variableName: "ConnectionStrings__SSO"),
            DecryptionKey = "000000000000000000000000000000000000000000000000",
        };

        ApplyEnvironment(settings: settings);
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
            try
            {
                if (databaseManager is not null)
                {
                    await databaseManager.DropDatabasesAsync();
                }
            }
            finally
            {
                RestoreEnvironment();
            }
        }
    }

    private void ApplyEnvironment(AcceptanceSettings settings)
    {
        previousEnvironmentValues = new Dictionary<string, string>
        {
            ["ConnectionStrings__Core"] = Environment.GetEnvironmentVariable(variable: "ConnectionStrings__Core"),
            ["ConnectionStrings__SSO"] = Environment.GetEnvironmentVariable(variable: "ConnectionStrings__SSO"),
            ["Settings__DecryptionKey"] = Environment.GetEnvironmentVariable(variable: "Settings__DecryptionKey"),
            ["Eventing__Http__HubUrl"] = Environment.GetEnvironmentVariable(variable: "Eventing__Http__HubUrl"),
        };

        Environment.SetEnvironmentVariable(variable: "ConnectionStrings__Core", value: settings.CoreConnectionString);
        Environment.SetEnvironmentVariable(variable: "ConnectionStrings__SSO", value: settings.SsoConnectionString);
        Environment.SetEnvironmentVariable(variable: "Settings__DecryptionKey", value: settings.DecryptionKey);
        Environment.SetEnvironmentVariable(variable: "Eventing__Http__HubUrl", value: string.Empty);
    }

    private void RestoreEnvironment()
    {
        if (previousEnvironmentValues is null)
        {
            return;
        }

        foreach ((string name, string value) in previousEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(variable: name, value: value);
        }
    }

    private static string AddDatabaseSuffix(string variableName)
    {
        string connectionString = ReadRequiredConnectionString(variableName: variableName);

        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true,
        };

        string databaseName = builder.InitialCatalog ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value: databaseName))
        {
            return connectionString;
        }

        builder.InitialCatalog = $"{databaseName}-acceptance-{Guid.NewGuid():N}";
        return builder.ConnectionString;
    }

    private static string ReadRequiredConnectionString(string variableName)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(variable: variableName)
            ?? Environment.GetEnvironmentVariable(variable: variableName, target: EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(variable: variableName, target: EnvironmentVariableTarget.Machine);

        if (!string.IsNullOrWhiteSpace(value: connectionString))
        {
            return connectionString;
        }

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