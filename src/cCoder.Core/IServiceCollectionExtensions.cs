// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddCoreWeb(
        this IServiceCollection services,
        Action<CoreConfiguration> configure = null)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(obj: configuration);

        return services.AddCoreWeb(
            configuration: configuration);
    }

    public static IServiceCollection AddCoreHostedServices(
        this IServiceCollection services,
        Action<CoreConfiguration> configure = null)
    {
        CoreConfiguration configuration = new();
        configure?.Invoke(obj: configuration);

        return services.AddCoreHostedServices(
            configuration: configuration);
    }

}