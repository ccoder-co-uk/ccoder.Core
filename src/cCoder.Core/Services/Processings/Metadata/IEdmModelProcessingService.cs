// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Services.Processings.Metadata;

internal interface IEdmModelProcessingService
{
    IEnumerable<ExtendedMetadataContainer> GetEdmModelMetadata(
        IEdmModel model,
        string contextName);

    ExtendedMetadataContainer GetExtendedMetadataContainer(
        IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true);
}