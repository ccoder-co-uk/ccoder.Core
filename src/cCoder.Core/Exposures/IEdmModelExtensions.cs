// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Core.Services.Processings.Metadata;
using cCoder.Core.Brokers.Metadata;
using cCoder.Core.Services.Foundations.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Exposures;

public static class IEdmModelExtensions
{
    public static IEnumerable<ExtendedMetadataContainer> GetMetadata(
        this IEdmModel model,
        string contextName) =>
        CreateEdmModelProcessingService()
            .GetEdmModelMetadata(
                model: model,
                contextName: contextName);

    public static ExtendedMetadataContainer GetExtendedMetadataForType(
        this IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true) =>
        CreateEdmModelProcessingService()
            .GetExtendedMetadataContainer(
                model: model,
                context: context,
                type: type,
                hasEndpoint: hasEndpoint);

    private static IEdmModelProcessingService CreateEdmModelProcessingService() =>
        new EdmModelProcessingService(
            edmModelService: new EdmModelService(
                edmModelBroker: new EdmModelBroker()));
}