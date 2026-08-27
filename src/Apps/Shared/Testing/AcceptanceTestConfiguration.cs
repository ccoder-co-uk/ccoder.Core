// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Data.SqlClient;

namespace cCoder.Core.Testing;

internal sealed class AcceptanceTestConfiguration
{
    private AcceptanceTestConfiguration(
        string coreConnectionString,
        string securityConnectionString,
        string decryptionKey)
    {
        CoreConnectionString = coreConnectionString;
        SecurityConnectionString = securityConnectionString;
        DecryptionKey = decryptionKey;
    }

    internal string CoreConnectionString { get; }

    internal string SecurityConnectionString { get; }

    internal string DecryptionKey { get; }

    internal IDisposable ApplyToProcess(bool aggregateDomains = false) =>
        ApplyToProcess(
            coreConnectionString: CoreConnectionString,
            securityConnectionString: SecurityConnectionString,
            decryptionKey: DecryptionKey,
            aggregateDomains: aggregateDomains);

    internal static IDisposable ApplyToProcess(
        string coreConnectionString,
        string securityConnectionString,
        string decryptionKey,
        bool aggregateDomains = false) =>
        new ProcessConfigurationScope(
            values: new Dictionary<string, string>
            {
                ["CoreData__ConnectionString"] = coreConnectionString,
                ["SecurityData__ConnectionString"] = securityConnectionString,
                ["AppSecurity__ConnectionString"] = coreConnectionString,
                ["AppSecurity__AggregateDomains"] =
                    aggregateDomains.ToString(),
                ["Security__ConnectionString"] = securityConnectionString,
                ["Security__DecryptionKey"] = decryptionKey,
                ["ContentManagement__ConnectionString"] =
                    coreConnectionString,
                ["DocumentManagement__ConnectionString"] =
                    coreConnectionString,
                ["Logging__ConnectionString"] = coreConnectionString,
                ["Mail__ConnectionString"] = coreConnectionString,
                ["Workflow__ConnectionString"] = coreConnectionString,
                ["Eventing__Http__HubUrl"] = string.Empty,
            });

    internal static AcceptanceTestConfiguration Load()
    {
        string suffix = $"-acceptance-{Guid.NewGuid():N}";

        return new AcceptanceTestConfiguration(
            coreConnectionString: AddDatabaseSuffix(
                connectionString: ReadRequiredValue(
                    variableName: "AppSecurity__ConnectionString"),
                suffix: suffix),
            securityConnectionString: AddDatabaseSuffix(
                connectionString: ReadRequiredValue(
                    variableName: "Security__ConnectionString"),
                suffix: suffix),
            decryptionKey: ReadRequiredValue(
                variableName: "Security__DecryptionKey"));
    }

    internal static bool ReadOptionalBool(string variableName) =>
        bool.TryParse(
            value: ReadOptionalValue(variableName: variableName),
            result: out bool value)
        && value;

    internal static int ReadOptionalInt(
        string variableName,
        int fallback) =>
        int.TryParse(
            s: ReadOptionalValue(variableName: variableName),
            result: out int value)
            ? value
            : fallback;

    internal static string ReadOptionalValue(string variableName) =>
        Environment.GetEnvironmentVariable(variable: variableName)
        ?? Environment.GetEnvironmentVariable(
            variable: variableName,
            target: EnvironmentVariableTarget.User)
        ?? Environment.GetEnvironmentVariable(
            variable: variableName,
            target: EnvironmentVariableTarget.Machine);

    internal static string ReadRequiredValue(string variableName)
    {
        string value = ReadOptionalValue(variableName: variableName);

        if (!string.IsNullOrWhiteSpace(value: value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Required configuration environment variable '{variableName}' was not found.");
    }

    private static string AddDatabaseSuffix(
        string connectionString,
        string suffix)
    {
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true,
        };

        if (string.IsNullOrWhiteSpace(value: builder.InitialCatalog))
        {
            throw new InvalidOperationException(
                "Acceptance test connection strings must name a database.");
        }

        builder.InitialCatalog = $"{builder.InitialCatalog}{suffix}";
        return builder.ConnectionString;
    }

    private sealed class ProcessConfigurationScope : IDisposable
    {
        private readonly Dictionary<string, string> originalValues;

        internal ProcessConfigurationScope(
            Dictionary<string, string> values)
        {
            originalValues = values.Keys.ToDictionary(
                keySelector: key => key,
                elementSelector: key =>
                    Environment.GetEnvironmentVariable(
                        variable: key,
                        target: EnvironmentVariableTarget.Process));

            foreach ((string key, string value) in values)
            {
                Environment.SetEnvironmentVariable(
                    variable: key,
                    value: value,
                    target: EnvironmentVariableTarget.Process);
            }
        }

        public void Dispose()
        {
            foreach ((string key, string value) in originalValues)
            {
                Environment.SetEnvironmentVariable(
                    variable: key,
                    value: value,
                    target: EnvironmentVariableTarget.Process);
            }
        }
    }

}