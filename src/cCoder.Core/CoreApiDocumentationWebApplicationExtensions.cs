using cCoder.Data.Models;
using Microsoft.AspNetCore.OData;


namespace cCoder.Core;

internal static class CoreApiDocumentationWebApplicationExtensions
{
    internal static WebApplication UseCoreApiDocumentation(this WebApplication app)
    {
        string[] contexts = ["Core", .. app.Services
            .GetServices<ApiInfo>()
            .Where(info => string.Equals(info.Kind, "Context", StringComparison.OrdinalIgnoreCase))
            .Select(info => info.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !string.Equals(name, "Core", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)];

        return app.UseCoreApiDocumentation(contexts);
    }

    internal static WebApplication UseCoreApiDocumentation(
        this WebApplication app,
        params string[] apiContexts
    )
    {
        string[] contexts = ["Core", .. (apiContexts ?? [])
            .Where(context => !string.IsNullOrWhiteSpace(context))
            .Distinct(StringComparer.OrdinalIgnoreCase)];

        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                foreach (string context in contexts)
                    options.SwaggerEndpoint($"/swagger/{context}/swagger.json", $"{context} API");

                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Core API");
            })
            .UseODataBatching()
            .UseODataRouteDebug();

        return app;
    }
}



