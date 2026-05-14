using cCoder.Core.Controllers;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;
using cCoder.Data;
using cCoder.Security;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Exposures;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreFirstTimeSetup(IServiceCollection services)
    {
        EnsureFirstTimeSetupSecurityServices(services);
        EnsureFirstTimeSetupSecurityManagers(services);
        services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateService>();
        services.AddScoped<Web.Services.Setup.IFirstTimeSetupStateService>(
            serviceProvider =>
                new Web.Services.Setup.LegacyFirstTimeSetupStateServiceAdapter(
                    serviceProvider.GetRequiredService<IFirstTimeSetupStateService>()));
        services.AddScoped<FirstTimeSetupAssetService>();
        services.AddScoped<BaselineAssetRepairService>();
        services.AddScoped<IFirstTimeSetupUserService, FirstTimeSetupUserService>();
        services.AddScoped<IFirstTimeSetupTenantService, FirstTimeSetupTenantService>();
        services.AddScoped<IFirstTimeSetupAppService, FirstTimeSetupAppService>();
        services.AddScoped<IFirstTimeSetupOrchestrationService, FirstTimeSetupOrchestrationService>();
        services.AddMvc().AddApplicationPart(typeof(SetupController).Assembly);
    }

    private static void EnsureFirstTimeSetupSecurityServices(IServiceCollection services)
    {
        if (HasServiceRegistration(
                services,
                "cCoder.Security.Services.Orchestrations.Interfaces.IAuthenticationOrchestrationService, cCoder.Security")
            && HasServiceRegistration(
                services,
                "cCoder.Security.Services.Foundations.Events.ITenantSetupEventService, cCoder.Security"))
        {
            return;
        }

        CoreConfiguration coreConfiguration = services
            .Where(descriptor => descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();
        Config runtimeConfiguration = services
            .Where(descriptor => descriptor.ServiceType == typeof(Config))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<Config>()
            .LastOrDefault();

        string securityConnectionString = coreConfiguration?.SecurityConnectionString ?? string.Empty;
        string decryptionKey = coreConfiguration?.DecryptionKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(securityConnectionString)
            && runtimeConfiguration?.ConnectionStrings?.TryGetValue("SSO", out string configuredSecurityConnection) == true)
        {
            securityConnectionString = configuredSecurityConnection;
        }

        if (string.IsNullOrWhiteSpace(decryptionKey)
            && runtimeConfiguration?.Settings?.TryGetValue("DecryptionKey", out string configuredDecryptionKey) == true)
        {
            decryptionKey = configuredDecryptionKey;
        }

        cCoder.Security.IServiceCollectionExtensions.AddSecurity(services, (securityServices, securityConfig) =>
        {
            securityConfig.RootPath = null;
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                securityConnectionString ?? string.Empty);
            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                decryptionKey ?? string.Empty);
        });
    }

    private static void EnsureFirstTimeSetupSecurityManagers(IServiceCollection services)
    {
        if (!services.Any(descriptor => descriptor.ServiceType == typeof(IAccountManager)))
        {
            Type accountManagerType = Type.GetType("cCoder.Security.Api.AccountManager, cCoder.Security");

            if (accountManagerType is not null)
                services.AddTransient(typeof(IAccountManager), accountManagerType);
        }

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(ITenantManager)))
        {
            Type tenantManagerType = Type.GetType("cCoder.Security.Exposures.TenantManager, cCoder.Security");

            if (tenantManagerType is not null)
                services.AddTransient(typeof(ITenantManager), tenantManagerType);
        }
    }

    private static bool HasServiceRegistration(IServiceCollection services, string assemblyQualifiedTypeName)
    {
        Type serviceType = Type.GetType(assemblyQualifiedTypeName);
        string fullName = assemblyQualifiedTypeName.Split(',')[0];

        return services.Any(descriptor =>
            descriptor.ServiceType == serviceType
            || string.Equals(descriptor.ServiceType.FullName, fullName, StringComparison.Ordinal));
    }
}
