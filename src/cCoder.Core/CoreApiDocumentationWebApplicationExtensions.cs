using Microsoft.AspNetCore.OData;


namespace cCoder.Core;

public static class CoreApiDocumentationWebApplicationExtensions
{
    public static WebApplication UseCoreApiDocumentation(
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



