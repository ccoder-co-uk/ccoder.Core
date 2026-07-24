// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Core.Services.Processings.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Exposures;

public static class IEdmModelExtensions
{
    public static IEnumerable<ExtendedMetadataContainer> GetMetadata(
        this IEdmModel model,
        string contextName) =>
        new EdmModelProcessingService()
            .GetEdmModelMetadata(
                model: model,
                contextName: contextName);

    public static ExtendedMetadataContainer GetExtendedMetadataForType(
        this IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true) =>
        new EdmModelProcessingService()
            .GetExtendedMetadataContainer(
                model: model,
                context: context,
                type: type,
                hasEndpoint: hasEndpoint);
}