// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
        Settings = new()
        {
            CoreConnectionString = AddDatabaseSuffix(variableName: "CCODER_ACCEPTANCE_CORE_CONNECTION_STRING"),
            SsoConnectionString = AddDatabaseSuffix(variableName: "CCODER_ACCEPTANCE_SSO_CONNECTION_STRING"),
            DecryptionKey = "000000000000000000000000000000000000000000000000",
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

    private static string AddDatabaseSuffix(string variableName)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(variable: variableName)
            ?? Environment.GetEnvironmentVariable(variable: variableName,target: EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(variable: variableName,target: EnvironmentVariableTarget.Machine)
            ?? ReadConfiguredConnectionString(variableName: variableName);

        if (string.IsNullOrWhiteSpace(value: connectionString))
        {
            return string.Empty;
        }

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

        string suffix = typeof(WebAcceptanceFixture).Assembly.GetName().Name!
            .Replace(oldValue: ".AcceptanceTests",newValue: string.Empty,comparisonType: StringComparison.Ordinal)
            .ToLowerInvariant();

        builder.InitialCatalog = $"{databaseName}-{suffix}";
        return builder.ConnectionString;
    }

    private static string ReadConfiguredConnectionString(string variableName)
    {
        string connectionName = variableName.Contains(value: "CORE",comparisonType: StringComparison.OrdinalIgnoreCase)
            ? "Core"
            : "SSO";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath: AppContext.BaseDirectory)
            .AddJsonFile(path: "appsettings.testing.json",optional: true)
            .Build();

        return configuration.GetConnectionString(name: connectionName) ?? string.Empty;
    }
}

[CollectionDefinition(Name)]
public sealed class WebAcceptanceCollection : ICollectionFixture<WebAcceptanceFixture>
{
    public const string Name = "Web acceptance";
}