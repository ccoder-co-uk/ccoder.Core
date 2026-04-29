using cCoder.Eventing.Models;

namespace cCoder.Core.Models;

public sealed class CoreConfiguration
{
    public string CoreConnectionString { get; set; } = string.Empty;
    public string SecurityConnectionString { get; set; } = string.Empty;
    public string SecurityRootPath { get; set; } = "Api/Security";
    public string DecryptionKey { get; set; } = string.Empty;
    public string CacheSource { get; set; } = string.Empty;
    public int? CacheSourceAppId { get; set; }
    public int? CacheExpiry { get; set; }
    public int? SslPort { get; set; }
    public string WorkflowServiceUrl { get; set; } = string.Empty;
    public string HttpEventHubUrl { get; set; } = string.Empty;
    public int MaxConcurrency { get; set; } = 1;
    public bool EnableHttpEventing { get; set; }
    public EventProvider[] EventProviders { get; set; } = [];
    public IDictionary<string, string> ConnectionStrings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> Services { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public bool DebugInfo { get; set; }
    public bool LogSQL { get; set; }
}
