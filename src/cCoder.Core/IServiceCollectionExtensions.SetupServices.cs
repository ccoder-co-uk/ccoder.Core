// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Exposures.Controllers;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;
using cCoder.Data;
using cCoder.Security;
using cCoder.Security.Data.EF;
using cCoder.Security.Exposures;
using cCoder.Security.Services.Orchestrations.Interfaces;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreFirstTimeSetup(IServiceCollection services)
    {
        EnsureFirstTimeSetupSecurityServices(services: services);
        EnsureFirstTimeSetupSecurityManagers(services: services);
        services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateService>();
        services.AddScoped<FirstTimeSetupAssetService>();
        services.AddScoped<IFirstTimeSetupUserService, FirstTimeSetupUserService>();
        services.AddScoped<IFirstTimeSetupTenantService, FirstTimeSetupTenantService>();
        services.AddScoped<IFirstTimeSetupAppService, FirstTimeSetupAppService>();
        services.AddScoped<IFirstTimeSetupOrchestrationService, FirstTimeSetupOrchestrationService>();
        services.AddMvc().AddApplicationPart(assembly: typeof(SetupController).Assembly);
    }

    private static void EnsureFirstTimeSetupSecurityServices(IServiceCollection services)
    {
        if (HasServiceRegistration(
services: services, assemblyQualifiedTypeName: "cCoder.Security.Services.Orchestrations.Interfaces.IAuthenticationOrchestrationService, cCoder.Security")
            && HasServiceRegistration(
services: services, assemblyQualifiedTypeName: "cCoder.Security.Services.Foundations.Events.ITenantSetupEventService, cCoder.Security"))
        {
            return;
        }

        CoreConfiguration coreConfiguration = services
            .Where(predicate: descriptor => descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();
        Config runtimeConfiguration = services
            .Where(predicate: descriptor => descriptor.ServiceType == typeof(Config))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<Config>()
            .LastOrDefault();

        string securityConnectionString = coreConfiguration?.SecurityConnectionString ?? string.Empty;
        string decryptionKey = coreConfiguration?.DecryptionKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value: securityConnectionString)
            && runtimeConfiguration?.ConnectionStrings?.TryGetValue(key: "SSO", value: out string configuredSecurityConnection) == true)
        {
            securityConnectionString = configuredSecurityConnection;
        }

        if (string.IsNullOrWhiteSpace(value: decryptionKey)
            && runtimeConfiguration?.Settings?.TryGetValue(key: "DecryptionKey", value: out string configuredDecryptionKey) == true)
        {
            decryptionKey = configuredDecryptionKey;
        }

        cCoder.Security.IServiceCollectionExtensions.AddSecurity(services: services, configAction: (securityServices, securityConfig) =>
        {
            securityConfig.RootPath = null;
            securityConfig.AddMSSQLModelProvider(
services: securityServices, connectionString: securityConnectionString ?? string.Empty);
            securityConfig.UseAESHMMACPasswordEncryption(
services: securityServices, decryptionKey: decryptionKey ?? string.Empty);
        });
    }

    private static void EnsureFirstTimeSetupSecurityManagers(IServiceCollection services)
    {
        if (!services.Any(predicate: descriptor => descriptor.ServiceType == typeof(ITokenManager)))
        {
            Type tokenManagerType = Type.GetType(typeName: "cCoder.Security.Exposures.TokenManager, cCoder.Security");

            if (tokenManagerType is not null)
            {
                services.AddTransient(serviceType: typeof(ITokenManager), implementationType: tokenManagerType);
            }
        }

        if (!services.Any(predicate: descriptor => descriptor.ServiceType == typeof(ITenantManager)))
        {
            Type tenantManagerType = Type.GetType(typeName: "cCoder.Security.Exposures.TenantManager, cCoder.Security");

            if (tenantManagerType is not null)
            {
                services.AddTransient(serviceType: typeof(ITenantManager), implementationType: tenantManagerType);
            }
        }
    }

    private static bool HasServiceRegistration(IServiceCollection services, string assemblyQualifiedTypeName)
    {
        Type serviceType = Type.GetType(typeName: assemblyQualifiedTypeName);
        string fullName = assemblyQualifiedTypeName.Split(separator: ',')[0];

        return services.Any(predicate: descriptor =>
            descriptor.ServiceType == serviceType
            || string.Equals(a: descriptor.ServiceType.FullName, b: fullName, comparisonType: StringComparison.Ordinal));
    }
}