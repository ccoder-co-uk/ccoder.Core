// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using cCoder.Core.Testing;
using HostedServices.AcceptanceTests.Models;
using HostedServicesProgram = HostedServices.Program;

namespace HostedServices.AcceptanceTests.Infrastructure;

internal sealed class HostedServicesAcceptanceFactory
        : WebApplicationFactory<HostedServicesProgram>
{
    private readonly IDisposable environmentScope;

    internal HostedServicesAcceptanceFactory(AcceptanceSettings settings) =>
        environmentScope = AcceptanceTestConfiguration.ApplyToProcess(
            coreConnectionString: settings.CoreConnectionString,
            securityConnectionString: settings.SsoConnectionString,
            decryptionKey: settings.DecryptionKey);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        =>
        builder.UseEnvironment(environment: "Acceptance");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing: disposing);

        if (disposing)
        {
            environmentScope.Dispose();
        }
    }
}