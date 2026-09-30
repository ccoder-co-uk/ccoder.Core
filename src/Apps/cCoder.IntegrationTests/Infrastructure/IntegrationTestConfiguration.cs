// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Data.SqlClient;
using System;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class IntegrationTestConfiguration
{
    private IntegrationTestConfiguration()
    {
        string suffix = $"-acceptance-{Guid.NewGuid():N}";

        CoreConnectionString = AddDatabaseSuffix(
            connectionString: ReadRequiredValue(
                variableName: "CoreData__ConnectionString"),
            suffix: suffix);
        SecurityConnectionString = AddDatabaseSuffix(
            connectionString: ReadRequiredValue(
                variableName: "SecurityData__ConnectionString"),
            suffix: suffix);
        DecryptionKey = ReadRequiredValue(
            variableName: "Security__DecryptionKey");
        EventProviderType =
            ReadOptionalValue(variableName: "Eventing__ProviderType")
            ?? "Http";
        ServiceBusConnectionString =
            ReadOptionalValue(
                variableName: "Eventing__ServiceBus__ConnectionString")
            ?? string.Empty;
        ServiceBusMaxConcurrency =
            ReadOptionalInt(
                variableName: "Eventing__ServiceBus__MaxConcurrency",
                fallback: 1);
        MailTenantId =
            ReadOptionalValue(
                variableName: "Mail__Providers__MicrosoftGraph__TenantId")
            ?? string.Empty;
        MailClientId =
            ReadOptionalValue(
                variableName: "Mail__Providers__MicrosoftGraph__ClientId")
            ?? string.Empty;
        MailClientSecret =
            ReadOptionalValue(
                variableName: "Mail__Providers__MicrosoftGraph__ClientSecret")
            ?? string.Empty;
        MailSendUser =
            ReadOptionalValue(variableName: "CoreIntegrationTests__MailSendUser")
            ?? string.Empty;
        MailReceiveUser =
            ReadOptionalValue(variableName: "CoreIntegrationTests__MailReceiveUser")
            ?? string.Empty;
        KeepArtifacts =
            ReadOptionalBool(variableName: "CoreIntegrationTests__KeepArtifacts");
        UseLocalWorkflow =
            ReadOptionalBool(variableName: "CoreIntegrationTests__UseLocalWorkflow");
        LocalWorkflowProject =
            ReadOptionalValue(variableName: "CoreIntegrationTests__LocalWorkflowProject")
            ?? string.Empty;
        LocalWorkflowActivitiesProject =
            ReadOptionalValue(
                variableName: "CoreIntegrationTests__LocalWorkflowActivitiesProject")
            ?? string.Empty;
        LocalWorkflowEngineProject =
            ReadOptionalValue(
                variableName: "CoreIntegrationTests__LocalWorkflowEngineProject")
            ?? string.Empty;
        UseLocalSecurity =
            ReadOptionalBool(variableName: "CoreIntegrationTests__UseLocalSecurity");
        UseLocalAppSecurity =
            ReadOptionalBool(variableName: "CoreIntegrationTests__UseLocalAppSecurity");
        UseLocalData =
            ReadOptionalBool(variableName: "CoreIntegrationTests__UseLocalData");
        UseLocalContentManagement =
            ReadOptionalBool(
                variableName: "CoreIntegrationTests__UseLocalContentManagement");
        LocalContentManagementProject =
            ReadOptionalValue(
                variableName: "CoreIntegrationTests__LocalContentManagementProject")
            ?? string.Empty;
        LocalSecurityAssemblyVersion =
            ReadOptionalValue(
                variableName: "CoreIntegrationTests__LocalSecurityAssemblyVersion")
            ?? string.Empty;
    }

    internal string CoreConnectionString { get; }

    internal string SecurityConnectionString { get; }

    internal string DecryptionKey { get; }

    internal string EventProviderType { get; }

    internal string ServiceBusConnectionString { get; }

    internal int ServiceBusMaxConcurrency { get; }

    internal string MailTenantId { get; }

    internal string MailClientId { get; }

    internal string MailClientSecret { get; }

    internal string MailSendUser { get; }

    internal string MailReceiveUser { get; }

    internal bool KeepArtifacts { get; }

    internal bool UseLocalWorkflow { get; }

    internal string LocalWorkflowProject { get; }

    internal string LocalWorkflowActivitiesProject { get; }

    internal string LocalWorkflowEngineProject { get; }

    internal bool UseLocalSecurity { get; }

    internal bool UseLocalAppSecurity { get; }

    internal bool UseLocalData { get; }

    internal bool UseLocalContentManagement { get; }

    internal string LocalContentManagementProject { get; }

    internal string LocalSecurityAssemblyVersion { get; }

    internal static IntegrationTestConfiguration Load() =>
        new();

    private static bool ReadOptionalBool(string variableName) =>
        bool.TryParse(
            value: ReadOptionalValue(variableName: variableName),
            result: out bool value)
        && value;

    private static int ReadOptionalInt(
        string variableName,
        int fallback) =>
        int.TryParse(
            s: ReadOptionalValue(variableName: variableName),
            result: out int value)
            ? value
            : fallback;

    private static string ReadOptionalValue(string variableName) =>
        Environment.GetEnvironmentVariable(variable: variableName)
        ?? Environment.GetEnvironmentVariable(
            variable: variableName,
            target: EnvironmentVariableTarget.User)
        ?? Environment.GetEnvironmentVariable(
            variable: variableName,
            target: EnvironmentVariableTarget.Machine);

    private static string ReadRequiredValue(string variableName)
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
        SqlConnectionStringBuilder builder = new(connectionString);

        if (string.IsNullOrWhiteSpace(value: builder.InitialCatalog))
        {
            throw new InvalidOperationException(
                "Integration test connection strings must name a database.");
        }

        builder.InitialCatalog = $"{builder.InitialCatalog}{suffix}";
        return builder.ConnectionString;
    }
}