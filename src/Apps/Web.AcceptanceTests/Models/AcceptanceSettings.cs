namespace Web.AcceptanceTests.Models;

internal sealed class AcceptanceSettings
{
    public string CoreConnectionString { get; init; } = string.Empty;

    public string SsoConnectionString { get; init; } = string.Empty;

    public string DecryptionKey { get; init; } = string.Empty;
}
