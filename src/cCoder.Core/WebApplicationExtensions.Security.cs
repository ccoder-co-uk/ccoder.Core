// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.AppSecurity.Api.OData;
using cCoder.AppSecurity.Brokers.Metadata;
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
        log?.LogInformation(message: "Initialising Security");

        IMetadataTypeCache metadataTypeCache = app.Services.GetRequiredService<IMetadataTypeCache>();

        if (!metadataTypeCache.Contains(scope: SecurityMetadataScope))
        {
            metadataTypeCache.Set(
scope: SecurityMetadataScope, typeSetPayloads: [
                    JsonSerializer.Serialize(value: new MetadataContainerSet
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
        CreateSecurityEntity(type: typeof(T));

    private static ExtendedMetadataContainer CreateSecurityEntity(Type type)
    {
        ExtendedMetadataContainer metadata =
            MetadataBroker.CreateExtendedMetadataContainer(
                type: type,
                isEntity: true,
                hasEndpoint: true);

        metadata.Category = SecurityMetadataScope;

        return metadata;
    }
}