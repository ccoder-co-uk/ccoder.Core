// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Workflow;

namespace Workflow;

internal static class Program
{
    private static async Task Main()
    {
        FunctionsApplicationBuilder builder =
            FunctionsApplication.CreateBuilder(args: []);

        builder.ConfigureFunctionsWebApplication();
        builder.Services.AddWorkflowApplication();

        await builder
            .Build()
            .RunAsync();
    }
}