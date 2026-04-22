using System.Text.Json;
using cCoder.ContentManagement.Api.OData;
using cCoder.Data.Exposures;
using cCoder.Security.Objects.Entities;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private const string SecurityMetadataScope = "Security";

    private static WebApplication UseCoreSecurityExposure(
        this WebApplication app,
        ILogger log = null)
    {
        log?.LogInformation("Initialising Security");

        IMetadataTypeCache metadataTypeCache = app.Services.GetRequiredService<IMetadataTypeCache>();

        if (!metadataTypeCache.Contains(SecurityMetadataScope))
        {
            metadataTypeCache.Set(
                SecurityMetadataScope,
                [
                    JsonSerializer.Serialize(new MetadataContainerSet
                    {
                        Name = SecurityMetadataScope,
                        UriBase = SecurityMetadataScope,
                        Types =
                        [
                            SecurityEntity<SSOUser>(),
                            SecurityEntity<SSORole>(),
                            SecurityEntity<SSOPrivilege>(),
                            SecurityEntity<Tenant>(),
                            SecurityEntity<TenantAnalysis>(),
                            SecurityEntity<UserEvent>(),
                            SecurityEntity<SSOUserRole>(),
                        ],
                    }),
                ]);
        }

        return app;
    }

    private static ExtendedMetadataContainer SecurityEntity<T>() =>
        new(typeof(T), isEntity: true, hasEndpoint: true)
        {
            Category = SecurityMetadataScope,
        };
}
