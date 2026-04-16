namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static IConfiguration GetRequiredConfiguration(IServiceCollection services)
    {
        IConfiguration configuration = services
            .Where(descriptor => typeof(IConfiguration).IsAssignableFrom(descriptor.ServiceType))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<IConfiguration>()
            .LastOrDefault();

        if (configuration is not null)
            return configuration;

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetService<IConfiguration>()
            ?? throw new InvalidOperationException(
                "IConfiguration must already be registered on the IServiceCollection before calling AddCoreWeb or AddCoreHostedServices.");
    }
}
