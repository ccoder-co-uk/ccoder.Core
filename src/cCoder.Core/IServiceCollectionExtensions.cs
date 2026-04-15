
using cCoder.Core.Logging;
using cCoder.Core.Models;
using EventLibrary.Models;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static void AddCoreWeb(
        this IServiceCollection services,
        Action<CoreWebOptions> configure = null)
    {
        CoreWebOptions options = new();
        configure?.Invoke(options);

        IConfiguration configuration = options.Configuration ?? GetRequiredConfiguration(services);
        ConfigureDefaultLogging(services, configuration);
        services.AddSingleton<ILoggerProvider, CoreWebSignalRLoggingProvider>();

        services.AddCoreApi(core => core
            .WithEventProviders(options.EventProviders)
            .UseDefaultBaseline(configuration));
    }

    public static void AddCoreHostedServices(
        this IServiceCollection services,
        Action<CoreHostedServicesOptions> configure = null)
    {
        CoreHostedServicesOptions options = new();
        configure?.Invoke(options);

        IConfiguration configuration = options.Configuration ?? GetRequiredConfiguration(services);
        ConfigureDefaultLogging(services, configuration);
        services.AddSingleton<ILoggerProvider, CoreHostedSignalRLoggingProvider>();

        services.AddCore(core => core
            .WithEventProviders(options.EventProviders)
            .UseDefaultBaseline(configuration));
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
    }
}
