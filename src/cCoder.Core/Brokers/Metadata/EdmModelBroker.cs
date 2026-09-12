// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Metadata;
using cCoder.Core.Models.Metadata;

namespace cCoder.Core.Brokers.Metadata;

internal sealed class EdmModelBroker : IEdmModelBroker
{
    public EdmModelDetails RetrieveEdmModelDetails(
        EdmModelDetails edmModelDetails) =>
        EdmModelDependency.RetrieveEdmModelDetails(
            edmModelDetails: edmModelDetails);
}