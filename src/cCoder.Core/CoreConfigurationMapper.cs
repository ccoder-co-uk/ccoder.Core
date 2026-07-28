// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Globalization;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Core.Models;
using cCoder.Data;
using cCoder.DocumentManagement.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Workflow.Models;

namespace cCoder.Core;

internal static class CoreConfigurationMapper
{
    internal static void ApplyBoundRootSections(CoreConfiguration target) =>
        PopulateFromRuntimeConfiguration(
            target: target,
            source: new cCoder.Data.Config
            {
                ConnectionStrings = target.ConnectionStrings,
                Settings = target.Settings,
                Services = target.Services,
                DebugInfo = target.DebugInfo,
                LogSQL = target.LogSQL,
            });

    internal static void ApplyDomainDefaults(CoreConfiguration target)
    {
        CoreConfiguration root = target;

        ApplyDomainDefaults(
            domainTarget: target.AppSecurity,
            connectionStrings: target.AppSecurity.ConnectionStrings,
            settings: target.AppSecurity.Settings,
            services: target.AppSecurity.Services);

        ApplyDomainDefaults(
            domainTarget: target.ContentManagement,
            connectionStrings: target.ContentManagement.ConnectionStrings,
            settings: target.ContentManagement.Settings,
            services: target.ContentManagement.Services);

        ApplyDomainDefaults(
            domainTarget: target.DocumentManagement,
            connectionStrings: target.DocumentManagement.ConnectionStrings,
            settings: target.DocumentManagement.Settings,
            services: target.DocumentManagement.Services);

        ApplyDomainDefaults(
            domainTarget: target.DomainLogging,
            connectionStrings: target.DomainLogging.ConnectionStrings,
            settings: target.DomainLogging.Settings,
            services: target.DomainLogging.Services);

        ApplyDomainDefaults(
            domainTarget: target.Mail,
            connectionStrings: target.Mail.ConnectionStrings,
            settings: target.Mail.Settings,
            services: target.Mail.Services);

        ApplyDomainDefaults(
            domainTarget: target.Workflow,
            connectionStrings: target.Workflow.ConnectionStrings,
            settings: target.Workflow.Settings,
            services: target.Workflow.Services);

        void ApplyDomainDefaults(
            object domainTarget,
            IDictionary<string, string> connectionStrings,
            IDictionary<string, string> settings,
            IDictionary<string, string> services)
        {
            MergeMissingEntries(target: connectionStrings, defaults: root.ConnectionStrings);
            MergeMissingEntries(target: settings, defaults: root.Settings);
            MergeMissingEntries(target: services, defaults: root.Services);

            switch (domainTarget)
            {
                case AppSecurityConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
                case ContentManagementConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
                case DocumentManagementConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
                case LoggingConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
                case MailConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
                case WorkflowConfiguration configuration:
                    configuration.DebugInfo |= root.DebugInfo;
                    configuration.LogSQL |= root.LogSQL;
                    break;
            }
        }
    }

    internal static void PopulateFromRuntimeConfiguration(
        CoreConfiguration target,
        cCoder.Data.Config source)
    {
        target.ConnectionStrings = CloneDictionary(source: source.ConnectionStrings);
        target.Settings = CloneDictionary(source: source.Settings);
        target.Services = CloneDictionary(source: source.Services);
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        if (TryGetValue(values: target.ConnectionStrings, key: "Core", value: out string coreConnectionString))
        {
            target.CoreConnectionString = coreConnectionString;
        }

        if (TryGetValue(values: target.ConnectionStrings, key: "SSO", value: out string securityConnectionString))
        {
            target.SecurityConnectionString = securityConnectionString;
        }

        if (TryGetBool(values: target.Settings, key: "AggregateDomains", value: out bool aggregateDomains))
        {
            target.AggregateDomains = aggregateDomains;
        }

        if (TryGetValue(values: target.Settings, key: "DecryptionKey", value: out string decryptionKey))
        {
            target.DecryptionKey = decryptionKey;
        }

        if (TryGetValue(values: target.Settings, key: "CacheSource", value: out string cacheSource))
        {
            target.CacheSource = cacheSource;
        }

        if (TryGetInt(values: target.Settings, key: "CacheSourceAppId", value: out int cacheSourceAppId))
        {
            target.CacheSourceAppId = cacheSourceAppId;
        }

        if (TryGetInt(values: target.Settings, key: "CacheExpiry", value: out int cacheExpiry))
        {
            target.CacheExpiry = cacheExpiry;
        }

        if (TryGetInt(values: target.Settings, key: "sslPort", value: out int sslPort))
        {
            target.SslPort = sslPort;
        }

        if (TryGetValue(values: target.Services, key: "Workflow", value: out string workflowServiceUrl))
        {
            target.WorkflowServiceUrl = workflowServiceUrl;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphTenantId", value: out string mailGraphTenantId))
        {
            target.MailGraphTenantId = mailGraphTenantId;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphClientId", value: out string mailGraphClientId))
        {
            target.MailGraphClientId = mailGraphClientId;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphClientSecret", value: out string mailGraphClientSecret))
        {
            target.MailGraphClientSecret = mailGraphClientSecret;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphBaseUrl", value: out string mailGraphBaseUrl))
        {
            target.MailGraphBaseUrl = mailGraphBaseUrl;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphLoginBaseUrl", value: out string mailGraphLoginBaseUrl))
        {
            target.MailGraphLoginBaseUrl = mailGraphLoginBaseUrl;
        }

        if (TryGetValue(values: target.Settings, key: "MailGraphReceiveUser", value: out string mailGraphReceiveUser))
        {
            target.MailGraphReceiveUser = mailGraphReceiveUser;
        }

        if (TryGetValue(values: target.Settings, key: "MailDefaultSenderProviderName", value: out string mailDefaultSenderProviderName))
        {
            target.MailDefaultSenderProviderName = mailDefaultSenderProviderName;
        }

        if (TryGetValue(values: target.Settings, key: "MailDefaultReceiverProviderName", value: out string mailDefaultReceiverProviderName))
        {
            target.MailDefaultReceiverProviderName = mailDefaultReceiverProviderName;
        }

        if (TryGetValue(values: target.ConnectionStrings, key: "ServiceBus", value: out string serviceBusConnectionString))
        {
            target.ServiceBusConnectionString = serviceBusConnectionString;
        }

        target.EnableHttpEventing =
            target.EnableHttpEventing
            || (string.Equals(a: target.EventProviderType, b: "Http", comparisonType: StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(value: target.HttpEventHubUrl));

        target.EnableServiceBusEventing =
            target.EnableServiceBusEventing
            || (string.Equals(a: target.EventProviderType, b: "ServiceBus", comparisonType: StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(value: target.ServiceBusConnectionString));
    }

    internal static void Copy(
        CoreConfiguration source,
        CoreConfiguration target)
    {
        target.CoreConnectionString = source.CoreConnectionString;
        target.SecurityConnectionString = source.SecurityConnectionString;
        target.SecurityRootPath = source.SecurityRootPath;
        target.AggregateDomains = source.AggregateDomains;
        target.DecryptionKey = source.DecryptionKey;
        target.CacheSource = source.CacheSource;
        target.CacheSourceAppId = source.CacheSourceAppId;
        target.CacheExpiry = source.CacheExpiry;
        target.SslPort = source.SslPort;
        target.WorkflowServiceUrl = source.WorkflowServiceUrl;
        target.MailGraphTenantId = source.MailGraphTenantId;
        target.MailGraphClientId = source.MailGraphClientId;
        target.MailGraphClientSecret = source.MailGraphClientSecret;
        target.MailGraphBaseUrl = source.MailGraphBaseUrl;
        target.MailGraphLoginBaseUrl = source.MailGraphLoginBaseUrl;
        target.MailGraphReceiveUser = source.MailGraphReceiveUser;
        target.MailDefaultSenderProviderName = source.MailDefaultSenderProviderName;
        target.MailDefaultReceiverProviderName = source.MailDefaultReceiverProviderName;
        target.EventProviderType = source.EventProviderType;
        target.HttpEventHubUrl = source.HttpEventHubUrl;
        target.ServiceBusConnectionString = source.ServiceBusConnectionString;
        target.MaxConcurrency = source.MaxConcurrency;
        target.EnableHttpEventing = source.EnableHttpEventing;
        target.EnableServiceBusEventing = source.EnableServiceBusEventing;
        target.EventProviders = source.EventProviders ?? [];
        target.ConnectionStrings = CloneDictionary(source: source.ConnectionStrings);
        target.Settings = CloneDictionary(source: source.Settings);
        target.Services = CloneDictionary(source: source.Services);
        target.DebugInfo = source.DebugInfo;
        target.LogSQL = source.LogSQL;
        target.AppSecurity = source.AppSecurity ?? new AppSecurityConfiguration();
        target.ContentManagement = source.ContentManagement ?? new ContentManagementConfiguration();
        target.DocumentManagement = source.DocumentManagement ?? new DocumentManagementConfiguration();
        target.DomainLogging = source.DomainLogging ?? new LoggingConfiguration();
        target.Mail = source.Mail ?? new MailConfiguration();
        target.Workflow = source.Workflow ?? new WorkflowConfiguration();
        target.Eventing = source.Eventing ?? new cCoder.Eventing.Models.EventingConfiguration();
    }

    internal static cCoder.Data.Config CreateRuntimeConfiguration(CoreConfiguration configuration) =>
        new()
        {
            ConnectionStrings = BuildConnectionStrings(configuration: configuration),
            Settings = BuildSettings(configuration: configuration),
            Services = BuildServices(configuration: configuration),
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
        {
            return;
        }

        SetIfMissing(values: connectionStrings, key: "Core", value: defaults.CoreConnectionString);
        SetIfMissing(values: connectionStrings, key: "SSO", value: defaults.SecurityConnectionString);
        SetIfMissing(values: connectionStrings, key: "ServiceBus", value: defaults.ServiceBusConnectionString);
        SetIfMissing(values: settings, key: "DecryptionKey", value: defaults.DecryptionKey);
        SetIfMissing(values: settings, key: "AggregateDomains", value: defaults.AggregateDomains);
        SetIfMissing(values: settings, key: "CacheSource", value: defaults.CacheSource);
        SetIfMissing(values: settings, key: "CacheSourceAppId", value: defaults.CacheSourceAppId);
        SetIfMissing(values: settings, key: "CacheExpiry", value: defaults.CacheExpiry);
        SetIfMissing(values: settings, key: "sslPort", value: defaults.SslPort);
        SetIfMissing(values: settings, key: "MailGraphTenantId", value: defaults.MailGraphTenantId);
        SetIfMissing(values: settings, key: "MailGraphClientId", value: defaults.MailGraphClientId);
        SetIfMissing(values: settings, key: "MailGraphClientSecret", value: defaults.MailGraphClientSecret);
        SetIfMissing(values: settings, key: "MailGraphBaseUrl", value: defaults.MailGraphBaseUrl);
        SetIfMissing(values: settings, key: "MailGraphLoginBaseUrl", value: defaults.MailGraphLoginBaseUrl);
        SetIfMissing(values: settings, key: "MailGraphReceiveUser", value: defaults.MailGraphReceiveUser);
        SetIfMissing(values: settings, key: "MailDefaultSenderProviderName", value: defaults.MailDefaultSenderProviderName);
        SetIfMissing(values: settings, key: "MailDefaultReceiverProviderName", value: defaults.MailDefaultReceiverProviderName);
        SetIfMissing(values: servicesMap, key: "Workflow", value: defaults.WorkflowServiceUrl);

        MergeMissingEntries(target: connectionStrings, defaults: defaults.ConnectionStrings);
        MergeMissingEntries(target: settings, defaults: defaults.Settings);
        MergeMissingEntries(target: servicesMap, defaults: defaults.Services);
        debugInfo(obj: currentDebugInfo || defaults.DebugInfo);
        logSql(obj: currentLogSql || defaults.LogSQL);
    }

    private static Dictionary<string, string> BuildConnectionStrings(CoreConfiguration configuration)
    {
        Dictionary<string, string> connectionStrings = CloneDictionary(source: configuration.ConnectionStrings);
        SetIfPresent(values: connectionStrings, key: "Core", value: configuration.CoreConnectionString);
        SetIfPresent(values: connectionStrings, key: "SSO", value: configuration.SecurityConnectionString);
        SetIfPresent(values: connectionStrings, key: "ServiceBus", value: configuration.ServiceBusConnectionString);
        return connectionStrings;
    }

    private static Dictionary<string, string> BuildSettings(CoreConfiguration configuration)
    {
        Dictionary<string, string> settings = CloneDictionary(source: configuration.Settings);
        SetIfPresent(values: settings, key: "DecryptionKey", value: configuration.DecryptionKey);
        SetIfPresent(values: settings, key: "AggregateDomains", value: configuration.AggregateDomains);
        SetIfPresent(values: settings, key: "CacheSource", value: configuration.CacheSource);
        SetIfPresent(values: settings, key: "CacheSourceAppId", value: configuration.CacheSourceAppId);
        SetIfPresent(values: settings, key: "CacheExpiry", value: configuration.CacheExpiry);
        SetIfPresent(values: settings, key: "sslPort", value: configuration.SslPort);
        SetIfPresent(values: settings, key: "MailGraphTenantId", value: configuration.MailGraphTenantId);
        SetIfPresent(values: settings, key: "MailGraphClientId", value: configuration.MailGraphClientId);
        SetIfPresent(values: settings, key: "MailGraphClientSecret", value: configuration.MailGraphClientSecret);
        SetIfPresent(values: settings, key: "MailGraphBaseUrl", value: configuration.MailGraphBaseUrl);
        SetIfPresent(values: settings, key: "MailGraphLoginBaseUrl", value: configuration.MailGraphLoginBaseUrl);
        SetIfPresent(values: settings, key: "MailGraphReceiveUser", value: configuration.MailGraphReceiveUser);
        SetIfPresent(values: settings, key: "MailDefaultSenderProviderName", value: configuration.MailDefaultSenderProviderName);
        SetIfPresent(values: settings, key: "MailDefaultReceiverProviderName", value: configuration.MailDefaultReceiverProviderName);
        return settings;
    }

    private static Dictionary<string, string> BuildServices(CoreConfiguration configuration)
    {
        Dictionary<string, string> services = CloneDictionary(source: configuration.Services);
        SetIfPresent(values: services, key: "Workflow", value: configuration.WorkflowServiceUrl);
        return services;
    }

    private static Dictionary<string, string> CloneDictionary(IDictionary<string, string> source) =>
        new(source ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);

    private static void MergeMissingEntries(
        IDictionary<string, string> target,
        IDictionary<string, string> defaults)
    {
        if (target is null || defaults is null)
        {
            return;
        }

        foreach ((string key, string value) in defaults)
        {
            if (!target.ContainsKey(key: key))
            {
                target[key] = value;
            }
        }
    }

    private static bool TryGetValue(
        IDictionary<string, string> values,
        string key,
        out string value)
    {
        value = string.Empty;

        return values?.TryGetValue(key: key, value: out value) == true
            && !string.IsNullOrWhiteSpace(value: value);
    }

    private static bool TryGetInt(
        IDictionary<string, string> values,
        string key,
        out int value)
    {
        value = default;

        return values?.TryGetValue(key: key, value: out string raw) == true
            && int.TryParse(s: raw, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out value);
    }

    private static bool TryGetBool(
        IDictionary<string, string> values,
        string key,
        out bool value)
    {
        value = default;

        return values?.TryGetValue(key: key, value: out string raw) == true
            && bool.TryParse(value: raw, result: out value);
    }

    private static void SetIfMissing(
        IDictionary<string, string> values,
        string key,
        string value)
    {
        if (values is null || string.IsNullOrWhiteSpace(value: key) || string.IsNullOrWhiteSpace(value: value) || values.ContainsKey(key: key))
        {
            return;
        }

        values[key] = value;
    }

    private static void SetIfMissing(
        IDictionary<string, string> values,
        string key,
        int? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        SetIfMissing(values: values, key: key, value: value.Value.ToString(provider: CultureInfo.InvariantCulture));
    }

    private static void SetIfMissing(
        IDictionary<string, string> values,
        string key,
        bool value) =>
        SetIfMissing(values: values, key: key, value: value ? "true" : "false");

    private static void SetIfPresent(
        IDictionary<string, string> values,
        string key,
        string value)
    {
        if (values is null || string.IsNullOrWhiteSpace(value: key) || string.IsNullOrWhiteSpace(value: value))
        {
            return;
        }

        values[key] = value;
    }

    private static void SetIfPresent(
        IDictionary<string, string> values,
        string key,
        int? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        SetIfPresent(values: values, key: key, value: value.Value.ToString(provider: CultureInfo.InvariantCulture));
    }

    private static void SetIfPresent(
        IDictionary<string, string> values,
        string key,
        bool value) =>
        SetIfPresent(values: values, key: key, value: value ? "true" : "false");
}