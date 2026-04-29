namespace cCoder.Core.Models;

public sealed class CoreHostConfiguration
{
    public string CoreConnectionString { get; init; } = string.Empty;
    public string SecurityConnectionString { get; init; } = string.Empty;
    public string DecryptionKey { get; init; } = string.Empty;
    public string CacheSource { get; init; } = string.Empty;
    public int? CacheSourceAppId { get; init; }
    public int? CacheExpiry { get; init; }
    public int? SslPort { get; init; }
    public string WorkflowServiceUrl { get; init; } = string.Empty;
    public string HttpEventHubUrl { get; init; } = string.Empty;
    public bool EnableHttpEventing { get; init; }
    public int MaxConcurrency { get; init; } = 1;
    public bool DebugInfo { get; init; }
    public bool LogSQL { get; init; }
}
