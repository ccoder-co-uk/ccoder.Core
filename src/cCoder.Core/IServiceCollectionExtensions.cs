
namespace cCoder.Core;

public static class IServiceCollectionExtensions
{
    public static void AddCoreData(
        this IServiceCollection services,
        string connectionString
    ) => cCoder.Data.IServiceCollectionExtensions.AddCoreData(
        services,
        connectionString);

    public static void AddCoreDataAccess(
        this IServiceCollection services,
        string connectionString
    ) => cCoder.Data.IServiceCollectionExtensions.AddCoreDataAccess(services, connectionString);

    public static void AddCoreAuthInfo(
        this IServiceCollection services
    ) => cCoder.Data.IServiceCollectionExtensions.AddCoreAuthInfo(services);

    public static void AddCore(
        this IServiceCollection services,
        Action<CoreBuilderOptions> setupAction
    )
    {
        CoreBuilderOptions config = new(services);
        setupAction(config);
    }
}




