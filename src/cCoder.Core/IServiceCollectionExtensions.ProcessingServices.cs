// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.AllowedOrigins;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    private static void AddCoreProcessingServices(IServiceCollection services) =>
        services.AddTransient<IAllowedOriginProcessingService, AllowedOriginProcessingService>();
}