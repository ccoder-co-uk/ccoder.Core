// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;

namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder =
            WebApplication.CreateBuilder(args: args);

        builder.Services.AddHostedServicesApp(builder: builder);

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.MapHostedServicesHealth();
        app.Run();
    }
}