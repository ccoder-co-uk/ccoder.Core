using cCoder.Core.Logging;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void ConfigureDefaultLogging(
        IServiceCollection services,
        IConfiguration configuration
    ) =>
        services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddFilter(level => level >= LogLevel.Debug);
            logBuilder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
                options.SingleLine = true;
            });
            logBuilder.AddConfiguration(configuration.GetSection("Logging"));
        });
}
