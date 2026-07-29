// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Testing;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class IntegrationTestConfiguration
{
    private IntegrationTestConfiguration(
        AcceptanceTestConfiguration acceptanceConfiguration)
    {
        Acceptance = acceptanceConfiguration;
        EventProviderType =
            AcceptanceTestConfiguration.ReadOptionalValue(
                variableName: "Eventing__ProviderType")
            ?? "Http";
        ServiceBusConnectionString =
            AcceptanceTestConfiguration.ReadOptionalValue(
                variableName:
                    "Eventing__ServiceBus__ConnectionString")
            ?? string.Empty;
        ServiceBusMaxConcurrency =
            AcceptanceTestConfiguration.ReadOptionalInt(
                variableName:
                    "Eventing__ServiceBus__MaxConcurrency",
                fallback: 1);
        MailTenantId =
            AcceptanceTestConfiguration.ReadRequiredValue(
                variableName: "Mail__MicrosoftGraph__TenantId");
        MailClientId =
            AcceptanceTestConfiguration.ReadRequiredValue(
                variableName: "Mail__MicrosoftGraph__ClientId");
        MailClientSecret =
            AcceptanceTestConfiguration.ReadRequiredValue(
                variableName:
                    "Mail__MicrosoftGraph__ClientSecret");
        MailSendUser =
            AcceptanceTestConfiguration.ReadRequiredValue(
                variableName: "Mail__MicrosoftGraph__SendUser");
        MailReceiveUser =
            AcceptanceTestConfiguration.ReadRequiredValue(
                variableName:
                    "Mail__MicrosoftGraph__ReceiveUser");
        KeepArtifacts =
            AcceptanceTestConfiguration.ReadOptionalBool(
                variableName:
                    "CoreIntegrationTests__KeepArtifacts");
        UseLocalWorkflow =
            AcceptanceTestConfiguration.ReadOptionalBool(
                variableName:
                    "CoreIntegrationTests__UseLocalWorkflow");
        UseLocalSecurity =
            AcceptanceTestConfiguration.ReadOptionalBool(
                variableName:
                    "CoreIntegrationTests__UseLocalSecurity");
        UseLocalAppSecurity =
            AcceptanceTestConfiguration.ReadOptionalBool(
                variableName:
                    "CoreIntegrationTests__UseLocalAppSecurity");
        UseLocalData =
            AcceptanceTestConfiguration.ReadOptionalBool(
                variableName:
                    "CoreIntegrationTests__UseLocalData");
        LocalSecurityAssemblyVersion =
            AcceptanceTestConfiguration.ReadOptionalValue(
                variableName:
                    "CoreIntegrationTests__LocalSecurityAssemblyVersion")
            ?? string.Empty;
    }

    internal AcceptanceTestConfiguration Acceptance { get; }

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

    internal bool UseLocalSecurity { get; }

    internal bool UseLocalAppSecurity { get; }

    internal bool UseLocalData { get; }

    internal string LocalSecurityAssemblyVersion { get; }

    internal static IntegrationTestConfiguration Load() =>
        new(
            acceptanceConfiguration:
                AcceptanceTestConfiguration.Load());
}