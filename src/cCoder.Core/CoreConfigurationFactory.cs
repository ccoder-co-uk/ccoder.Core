// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Eventing.Models;
using Microsoft.Extensions.Configuration;

namespace cCoder.Core;

public static class CoreConfigurationFactory
{
    public static CoreConfiguration Create() =>
        new()
        {
            Eventing = new EventingConfiguration(),
            Api = new ApiConfiguration(),
        };

    public static CoreConfiguration Create(IConfiguration configuration) =>
        Create<CoreConfiguration>(configuration: configuration);

    public static TConfiguration Create<TConfiguration>(
        IConfiguration configuration)
        where TConfiguration : CoreConfiguration, new()
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);

        TConfiguration result = configuration.Get<TConfiguration>() ?? new();
        result.Eventing ??= new EventingConfiguration();
        result.Api ??= new ApiConfiguration();

        result.ApplicationConfiguration = configuration;

        return result;
    }

}