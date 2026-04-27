
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
        services.AddCoreApi(configure ?? (_ => { }));
    }

    public static void AddCoreHostedServices(
        this IServiceCollection services,
        Action<CoreBuilderOptions> configure = null)
    {
        ConfigureDefaultLogging(services, GetRequiredConfiguration(services));
        services.AddSingleton<ILoggerProvider, CoreHostedSignalRLoggingProvider>();
        services.AddCore(configure ?? (_ => { }));
        cCoder.Core.Api.IServiceCollectionExtensions.AddAspNet(services);
    }

    public static void AddCoreApi(
        this IServiceCollection services,
        Action<CoreApiBuilderOptions> setupAction)
    {
        CoreApiBuilderOptions config = new(services);
        setupAction(config);
        config.Apply();
    }

    public static void AddCore(
        this IServiceCollection services,
        Action<CoreBuilderOptions> setupAction
    )
    {
        CoreBuilderOptions config = new(services);
        setupAction(config);
        config.Apply();
    }
}
