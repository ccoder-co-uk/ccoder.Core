using cCoder.Data;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects;
using HostedServices.AcceptanceTests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal static class AcceptanceServiceProviderFactory
{
    public static ServiceProvider Create(AcceptanceSettings settings)
    {
        ServiceCollection services = new();
        services.AddLogging();

        services.AddSingleton(
            new Config
            {
                ConnectionStrings = new Dictionary<string, string>
                {
                    ["Core"] = settings.CoreConnectionString,
                    ["SSO"] = settings.SsoConnectionString
                },
                Settings = new Dictionary<string, string>
                {
                    ["DecryptionKey"] = settings.DecryptionKey
                },
                Services = new Dictionary<string, string>()
            });

        services.AddScoped<ISecurityDbContextFactory>(
            provider => new MSSQLSecurityDbContextFactory(settings.SsoConnectionString)
            {
                GetAuthInfo = ignoreAuthInfo => ignoreAuthInfo
                    ? new SSOAuthInfo { SSOUserId = "Guest" }
                    : provider.GetService<ISSOAuthInfo>()
            });

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services, settings.CoreConnectionString);

        return services.BuildServiceProvider(validateScopes: false);
    }
}
