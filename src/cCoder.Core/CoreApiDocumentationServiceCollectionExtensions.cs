using Microsoft.OpenApi;


namespace cCoder.Core;

public static class CoreApiDocumentationServiceCollectionExtensions
{
    public static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        params string[] apiContexts)
    {
        string[] contexts = GetApiContexts(apiContexts);

        services.AddSwaggerGen(c =>
        {
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            AddSwaggerDocuments(c, contexts);
            c.DocInclusionPredicate(
                (documentName, apiDescription) =>
                    ShouldIncludeInDocument(documentName, apiDescription.RelativePath, contexts));

            c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Description = @"Authorization header using the Bearer scheme. \r\n\r\n 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
            });
        });

        return services;
    }

    private static string[] GetApiContexts(IEnumerable<string> apiContexts) =>
        ["Core", .. (apiContexts ?? [])
            .Where(context => !string.IsNullOrWhiteSpace(context))
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private static void AddSwaggerDocuments(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        IEnumerable<string> contexts)
    {
        foreach (string context in contexts)
        {
            options.SwaggerDoc(context, new OpenApiInfo
            {
                Title = $"{context} API definition",
                Version = context,
            });
        }

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Corporate LinX V7 API definition",
            Version = "v1",
        });
    }

    private static bool ShouldIncludeInDocument(
        string documentName,
        string relativePath,
        IEnumerable<string> contexts)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        if (string.Equals(documentName, "v1", StringComparison.OrdinalIgnoreCase))
            documentName = "Core";

        string path = NormalizePath(relativePath);

        if (string.Equals(documentName, "Core", StringComparison.OrdinalIgnoreCase))
            return IsCoreRoute(path, contexts);

        return MatchesContextRoute(path, documentName);
    }

    private static bool IsCoreRoute(string path, IEnumerable<string> contexts)
    {
        if (MatchesContextRoute(path, "Core"))
            return true;

        if (!path.Equals("/Api", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("/Api/", StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (string context in contexts.Where(context =>
                     !string.Equals(context, "Core", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(context, "v1", StringComparison.OrdinalIgnoreCase)))
        {
            if (MatchesContextRoute(path, context))
                return false;
        }

        return true;
    }

    private static bool MatchesContextRoute(string path, string context)
    {
        string prefix = $"/Api/{context}";
        return path.Equals(prefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string relativePath) =>
        relativePath.StartsWith('/') ? relativePath : $"/{relativePath}";
}



