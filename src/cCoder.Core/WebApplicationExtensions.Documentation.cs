// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using Microsoft.AspNetCore.OData;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    internal static WebApplication UseCoreApiDocumentation(this WebApplication app)
    {
        string[] contexts = ["Core", .. app.Services
            .GetServices<ApiInfo>()
            .Where(predicate: info => string.Equals(a: info.Kind,b: "Context",comparisonType: StringComparison.OrdinalIgnoreCase))
            .Select(selector: info => info.Name)
            .Where(predicate: name => !string.IsNullOrWhiteSpace(value: name))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .Where(predicate: name => !string.Equals(a: name,b: "Core",comparisonType: StringComparison.OrdinalIgnoreCase))
            .OrderBy(keySelector: name => name,comparer: StringComparer.OrdinalIgnoreCase)];

        return app.UseCoreApiDocumentation(apiContexts: contexts);
    }

    internal static WebApplication UseCoreApiDocumentation(
        this WebApplication app,
        params string[] apiContexts
    )
    {
        string[] contexts = ["Core", .. (apiContexts ?? [])
            .Where(predicate: context => !string.IsNullOrWhiteSpace(value: context))
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .Where(predicate: context => !string.Equals(a: context,b: "Core",comparisonType: StringComparison.OrdinalIgnoreCase))];

        app.UseSwagger()
            .UseSwaggerUI(setupAction: options =>
            {
                foreach (string context in contexts)
                {
                    options.SwaggerEndpoint(url: $"/swagger/{context}/swagger.json", name: $"{context} API");
                }
            })
            .UseODataBatching()
            .UseODataRouteDebug();

        return app;
    }
}