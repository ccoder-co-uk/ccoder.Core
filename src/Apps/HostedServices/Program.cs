using cCoder.Core;


namespace HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureApplication(builder.Configuration);
        builder.Services.AddCoreHostedServices();

        WebApplication app = builder.Build();
        app.StartCoreHostedServices();
        app.Run();
    }

    private static void ConfigureApplication(ConfigurationManager configuration)
    {
        configuration
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.testing.json", optional: true, reloadOnChange: true);
    }
}



