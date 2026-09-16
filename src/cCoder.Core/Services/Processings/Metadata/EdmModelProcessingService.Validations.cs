// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using Microsoft.OData.Edm;

namespace cCoder.Core.Services.Processings.Metadata;

internal sealed partial class EdmModelProcessingService
{
    private static void Validate(params object[] inputs) =>
        ValidationRulesEngine.Validate(inputs: inputs);

    private static void ValidateEdmModelMetadataOnGet(
        IEdmModel model,
        string contextName) =>
        Validate(inputs: [model, contextName]);

    private static void ValidateExtendedMetadataContainerOnGet(
        IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint) =>
        Validate(
            inputs: [model, context, type, hasEndpoint]);
}