namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreApi(
        IServiceCollection services,
        Action<CoreApiBuilderOptions> setupAction)
    {
        CoreApiBuilderOptions config = new(services);
        setupAction(config);
        config.Apply();
    }

    private static void AddCore(
        IServiceCollection services,
        Action<CoreBuilderOptions> setupAction
    )
    {
        CoreBuilderOptions config = new(services);
        setupAction(config);
        config.Apply();
    }
}
