// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing.Models;

namespace cCoder.Core.Models;

public sealed class CoreConfiguration
{
    public CoreConfiguration()
    {
        CoreConnectionString = string.Empty;
        SecurityConnectionString = string.Empty;
        SecurityRootPath = "Api/Security";
        DecryptionKey = string.Empty;
        CacheSource = string.Empty;
        WorkflowServiceUrl = string.Empty;
        MailGraphTenantId = string.Empty;
        MailGraphClientId = string.Empty;
        MailGraphClientSecret = string.Empty;
        MailGraphBaseUrl = string.Empty;
        MailGraphLoginBaseUrl = string.Empty;
        MailGraphReceiveUser = string.Empty;
        MailDefaultSenderProviderName = string.Empty;
        MailDefaultReceiverProviderName = string.Empty;
        EventProviderType = "Http";
        HttpEventHubUrl = string.Empty;
        ServiceBusConnectionString = string.Empty;
        MaxConcurrency = 1;
        EventProviders = [];
        ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Services = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public string CoreConnectionString { get; set; }
    public string SecurityConnectionString { get; set; }
    public string SecurityRootPath { get; set; }
    public bool AggregateDomains { get; set; }
    public string DecryptionKey { get; set; }
    public string CacheSource { get; set; }
    public int? CacheSourceAppId { get; set; }
    public int? CacheExpiry { get; set; }
    public int? SslPort { get; set; }
    public string WorkflowServiceUrl { get; set; }
    public string MailGraphTenantId { get; set; }
    public string MailGraphClientId { get; set; }
    public string MailGraphClientSecret { get; set; }
    public string MailGraphBaseUrl { get; set; }
    public string MailGraphLoginBaseUrl { get; set; }
    public string MailGraphReceiveUser { get; set; }
    public string MailDefaultSenderProviderName { get; set; }
    public string MailDefaultReceiverProviderName { get; set; }
    public string EventProviderType { get; set; }
    public string HttpEventHubUrl { get; set; }
    public string ServiceBusConnectionString { get; set; }
    public int MaxConcurrency { get; set; }
    public bool EnableHttpEventing { get; set; }
    public bool EnableServiceBusEventing { get; set; }
    public EventProvider[] EventProviders { get; set; }
    public IDictionary<string, string> ConnectionStrings { get; set; }
    public IDictionary<string, string> Settings { get; set; }
    public IDictionary<string, string> Services { get; set; }
    public bool DebugInfo { get; set; }
    public bool LogSQL { get; set; }
}