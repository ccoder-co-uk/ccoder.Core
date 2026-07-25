// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using Microsoft.OData.Edm;

namespace cCoder.Core.Services.Processings.Metadata;

internal sealed partial class EdmModelProcessingService
{
    private static void ValidateEdmModelMetadataOnGet(
        IEdmModel model,
        string contextName) =>
        ValidationRulesEngine.Validate(inputs: [model, contextName]);

    private static void ValidateExtendedMetadataContainerOnGet(
        IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint) =>
        ValidationRulesEngine.Validate(
            inputs: [model, context, type, hasEndpoint]);
}