// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder =
            WebApplication.CreateBuilder(args: args);

        builder.Services.AddWeb(builder: builder);

        WebApplication app = builder.Build();
        app.StartCoreWeb();
        app.Run();
    }
}