// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        builder.Services.AddWorkflow(
            configuration: builder.Configuration);

        await builder
            .Build()
            .RunAsync();
    }
}