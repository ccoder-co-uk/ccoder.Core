using cCoder.Core.Data;
using cCoder.Core.Objects;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Scheduler.EF_Setup;

public class CoreContextFactory : IDesignTimeDbContextFactory<CoreDataContext>
{
    private readonly Config config = null;

    public CoreContextFactory()
    {
        config = new();

        new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "ENV_")
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Hosting:Environment")}.json", optional: true, reloadOnChange: true)
            .Build()
            .Bind(config);
    }

    public CoreDataContext CreateDbContext(string[] args) => new(null, config, null);
}