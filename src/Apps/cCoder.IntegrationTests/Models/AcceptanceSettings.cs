namespace cCoder.IntegrationTests.Models;

internal sealed class AcceptanceSettings
{
    public string CoreConnectionString { get; init; } = string.Empty;

    public string SsoConnectionString { get; init; } = string.Empty;

    public string DecryptionKey { get; init; } = string.Empty;

    public string EventProviderType { get; init; } = "Http";

    public string ServiceBusConnectionString { get; init; } = string.Empty;

    public int ServiceBusMaxConcurrency { get; init; } = 1;

    public bool UseServiceBusEventing =>
        string.Equals(EventProviderType, "ServiceBus", StringComparison.OrdinalIgnoreCase);
}
