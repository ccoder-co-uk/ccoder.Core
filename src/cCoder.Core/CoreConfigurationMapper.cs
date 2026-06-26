using System.Globalization;
using cCoder.Core.Models;
using cCoder.Data;

namespace cCoder.Core;

internal static class CoreConfigurationMapper
{
    internal static void PopulateFromRuntimeConfiguration(
        CoreConfiguration target,
        Config source)
    {
        target.ConnectionStrings = CloneDictionary(source.ConnectionStrings);
        target.Settings = CloneDictionary(source.Settings);
        target.Services = CloneDictionary(source.Services);
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;

        if (TryGetValue(target.ConnectionStrings, "Core", out string coreConnectionString))
            target.CoreConnectionString = coreConnectionString;

        if (TryGetValue(target.ConnectionStrings, "SSO", out string securityConnectionString))
            target.SecurityConnectionString = securityConnectionString;

        if (TryGetValue(target.Settings, "DecryptionKey", out string decryptionKey))
            target.DecryptionKey = decryptionKey;

        if (TryGetValue(target.Settings, "CacheSource", out string cacheSource))
            target.CacheSource = cacheSource;

        if (TryGetInt(target.Settings, "CacheSourceAppId", out int cacheSourceAppId))
            target.CacheSourceAppId = cacheSourceAppId;

        if (TryGetInt(target.Settings, "CacheExpiry", out int cacheExpiry))
            target.CacheExpiry = cacheExpiry;

        if (TryGetInt(target.Settings, "sslPort", out int sslPort))
            target.SslPort = sslPort;

        if (TryGetValue(target.Services, "Workflow", out string workflowServiceUrl))
            target.WorkflowServiceUrl = workflowServiceUrl;

        if (TryGetValue(target.ConnectionStrings, "ServiceBus", out string serviceBusConnectionString))
            target.ServiceBusConnectionString = serviceBusConnectionString;

        target.EnableHttpEventing =
            string.Equals(target.EventProviderType, "Http", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(target.HttpEventHubUrl);

        target.EnableServiceBusEventing =
            string.Equals(target.EventProviderType, "ServiceBus", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(target.ServiceBusConnectionString);
    }

    internal static void Copy(
        CoreConfiguration source,
        CoreConfiguration target)
    {
        target.CoreConnectionString = source.CoreConnectionString;
        target.SecurityConnectionString = source.SecurityConnectionString;
        target.SecurityRootPath = source.SecurityRootPath;
        target.DecryptionKey = source.DecryptionKey;
        target.CacheSource = source.CacheSource;
        target.CacheSourceAppId = source.CacheSourceAppId;
        target.CacheExpiry = source.CacheExpiry;
        target.SslPort = source.SslPort;
        target.WorkflowServiceUrl = source.WorkflowServiceUrl;
        target.EventProviderType = source.EventProviderType;
        target.HttpEventHubUrl = source.HttpEventHubUrl;
        target.ServiceBusConnectionString = source.ServiceBusConnectionString;
        target.MaxConcurrency = source.MaxConcurrency;
        target.EnableHttpEventing = source.EnableHttpEventing;
        target.EnableServiceBusEventing = source.EnableServiceBusEventing;
        target.EventProviders = source.EventProviders ?? [];
        target.ConnectionStrings = CloneDictionary(source.ConnectionStrings);
        target.Settings = CloneDictionary(source.Settings);
        target.Services = CloneDictionary(source.Services);
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
    }

    internal static Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        new()
        {
            ConnectionStrings = BuildConnectionStrings(configuration),
            Settings = BuildSettings(configuration),
            Services = BuildServices(configuration),
            DebugInfo = configuration.DebugInfo,
            LogSQL = configuration.LogSQL,
        };

    internal static void ApplyDefaults(
        CoreConfiguration defaults,
        IDictionary<string, string> connectionStrings,
        IDictionary<string, string> settings,
        IDictionary<string, string> servicesMap,
        Action<bool> debugInfo,
        Action<bool> logSql,
        bool currentDebugInfo,
        bool currentLogSql)
    {
        if (defaults is null)
            return;

        SetIfMissing(connectionStrings, "Core", defaults.CoreConnectionString);
        SetIfMissing(connectionStrings, "SSO", defaults.SecurityConnectionString);
        SetIfMissing(connectionStrings, "ServiceBus", defaults.ServiceBusConnectionString);
        SetIfMissing(settings, "DecryptionKey", defaults.DecryptionKey);
        SetIfMissing(settings, "CacheSource", defaults.CacheSource);
        SetIfMissing(settings, "CacheSourceAppId", defaults.CacheSourceAppId);
        SetIfMissing(settings, "CacheExpiry", defaults.CacheExpiry);
        SetIfMissing(settings, "sslPort", defaults.SslPort);
        SetIfMissing(servicesMap, "Workflow", defaults.WorkflowServiceUrl);

        MergeMissingEntries(connectionStrings, defaults.ConnectionStrings);
        MergeMissingEntries(settings, defaults.Settings);
        MergeMissingEntries(servicesMap, defaults.Services);
        debugInfo(currentDebugInfo || defaults.DebugInfo);
        logSql(currentLogSql || defaults.LogSQL);
    }

    private static Dictionary<string, string> BuildConnectionStrings(CoreConfiguration configuration)
    {
        Dictionary<string, string> connectionStrings = CloneDictionary(configuration.ConnectionStrings);
        SetIfPresent(connectionStrings, "Core", configuration.CoreConnectionString);
        SetIfPresent(connectionStrings, "SSO", configuration.SecurityConnectionString);
        SetIfPresent(connectionStrings, "ServiceBus", configuration.ServiceBusConnectionString);
        return connectionStrings;
    }

    private static Dictionary<string, string> BuildSettings(CoreConfiguration configuration)
    {
        Dictionary<string, string> settings = CloneDictionary(configuration.Settings);
        SetIfPresent(settings, "DecryptionKey", configuration.DecryptionKey);
        SetIfPresent(settings, "CacheSource", configuration.CacheSource);
        SetIfPresent(settings, "CacheSourceAppId", configuration.CacheSourceAppId);
        SetIfPresent(settings, "CacheExpiry", configuration.CacheExpiry);
        SetIfPresent(settings, "sslPort", configuration.SslPort);
        return settings;
    }

    private static Dictionary<string, string> BuildServices(CoreConfiguration configuration)
    {
        Dictionary<string, string> services = CloneDictionary(configuration.Services);
        SetIfPresent(services, "Workflow", configuration.WorkflowServiceUrl);
        return services;
    }

    private static Dictionary<string, string> CloneDictionary(IDictionary<string, string> source) =>
        new(source ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);

    private static void MergeMissingEntries(
        IDictionary<string, string> target,
        IDictionary<string, string> defaults)
    {
        if (target is null || defaults is null)
            return;

        foreach ((string key, string value) in defaults)
        {
            if (!target.ContainsKey(key))
                target[key] = value;
        }
    }

    private static bool TryGetValue(
        IDictionary<string, string> values,
        string key,
        out string value)
    {
        value = string.Empty;

        return values?.TryGetValue(key, out value) == true
            && !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetInt(
        IDictionary<string, string> values,
        string key,
        out int value)
    {
        value = default;

        return values?.TryGetValue(key, out string raw) == true
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static void SetIfMissing(
        IDictionary<string, string> values,
        string key,
        string value)
    {
        if (values is null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value) || values.ContainsKey(key))
            return;

        values[key] = value;
    }

    private static void SetIfMissing(
        IDictionary<string, string> values,
        string key,
        int? value)
    {
        if (!value.HasValue)
            return;

        SetIfMissing(values, key, value.Value.ToString(CultureInfo.InvariantCulture));
    }

    private static void SetIfPresent(
        IDictionary<string, string> values,
        string key,
        string value)
    {
        if (values is null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            return;

        values[key] = value;
    }

    private static void SetIfPresent(
        IDictionary<string, string> values,
        string key,
        int? value)
    {
        if (!value.HasValue)
            return;

        SetIfPresent(values, key, value.Value.ToString(CultureInfo.InvariantCulture));
    }
}
