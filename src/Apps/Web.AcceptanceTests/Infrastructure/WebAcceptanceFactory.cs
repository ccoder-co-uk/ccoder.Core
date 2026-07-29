// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cCoder.Core.Testing;
using Web.AcceptanceTests.Models;

namespace Web.AcceptanceTests.Infrastructure;

internal sealed class WebAcceptanceFactory : WebApplicationFactory<Program>
{
    private readonly IDisposable environmentScope;

    internal WebAcceptanceFactory(AcceptanceSettings settings) =>
        environmentScope = AcceptanceTestConfiguration.ApplyToProcess(
            coreConnectionString: settings.CoreConnectionString,
            securityConnectionString: settings.SsoConnectionString,
            decryptionKey: settings.DecryptionKey,
            aggregateDomains: settings.AggregateDomains);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment: "Acceptance");

        builder.ConfigureServices(configureServices: services =>
        {
            services.AddOptions();

            services.Replace(
                descriptor: ServiceDescriptor.Singleton<
                    IDistributedCache,
                    MemoryDistributedCache>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing: disposing);

        if (disposing)
        {
            environmentScope.Dispose();
        }
    }
}