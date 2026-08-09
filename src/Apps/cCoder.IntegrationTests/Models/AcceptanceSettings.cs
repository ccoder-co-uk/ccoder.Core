// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.IntegrationTests.Models;

internal sealed class AcceptanceSettings
{
    public string CoreConnectionString { get; init; } = string.Empty;

    public string SsoConnectionString { get; init; } = string.Empty;

    public string DecryptionKey { get; init; } = string.Empty;

    public string EventProviderType { get; init; } = "Http";

    public string ServiceBusConnectionString { get; init; } = string.Empty;

    public int ServiceBusMaxConcurrency { get; init; } = 1;

    public string MailTenantId { get; init; } = string.Empty;

    public string MailClientId { get; init; } = string.Empty;

    public string MailClientSecret { get; init; } = string.Empty;

    public string MailSendUser { get; init; } = string.Empty;

    public string MailReceiveUser { get; init; } = string.Empty;

    public bool KeepArtifacts { get; init; }

    public bool UseLocalWorkflow { get; init; }

    public bool UseLocalSecurity { get; init; }

    public bool UseLocalAppSecurity { get; init; }

    public bool UseLocalData { get; init; }

    public bool UseLocalContentManagement { get; init; }

    public string LocalContentManagementProject { get; init; } =
        string.Empty;

    public string LocalSecurityAssemblyVersion { get; init; } =
        string.Empty;

    public bool UseServiceBusEventing =>
        string.Equals(a: EventProviderType,b: "ServiceBus",comparisonType: StringComparison.OrdinalIgnoreCase);
}