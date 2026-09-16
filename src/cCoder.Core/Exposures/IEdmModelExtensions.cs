// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Exposures;

public static class IEdmModelExtensions
{
    public static IEnumerable<ExtendedMetadataContainer> GetMetadata(
        this IEdmModel model,
        string contextName) =>
        WebApplicationExtensions.CreateEdmModelProcessingService()
            .GetEdmModelMetadata(
                model: model,
                contextName: contextName);

    public static ExtendedMetadataContainer GetExtendedMetadataForType(
        this IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true) =>
        WebApplicationExtensions.CreateEdmModelProcessingService()
            .GetExtendedMetadataContainer(
                model: model,
                context: context,
                type: type,
                hasEndpoint: hasEndpoint);

}