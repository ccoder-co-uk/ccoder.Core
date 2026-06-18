
using cCoder.Core.Logging;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static void AddCoreWeb(
        this IServiceCollection services,
        Action<CoreApiBuilderOptions> configure = null)
    {
        ConfigureDefaultLogging(services, GetRequiredConfiguration(services));
        services.AddSingleton<ILoggerProvider, CoreWebSignalRLoggingProvider>();
        AddCoreApi(services, configure ?? (_ => { }));
        AddCoreFirstTimeSetup(services);
    }

    public static void AddCoreHostedServices(
        this IServiceCollection services,
        Action<CoreBuilderOptions> configure = null)
    {
        ConfigureDefaultLogging(services, GetRequiredConfiguration(services));
        services.AddSingleton<ILoggerProvider, CoreHostedSignalRLoggingProvider>();
        AddCore(services, configure ?? (_ => { }));
        cCoder.Core.Api.IServiceCollectionExtensions.AddAspNet(services);
    }
}