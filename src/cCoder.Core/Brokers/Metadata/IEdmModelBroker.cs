// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;

namespace cCoder.Core.Brokers.Metadata;

internal interface IEdmModelBroker
{
    EdmModelDetails RetrieveEdmModelDetails(EdmModelDetails edmModelDetails);
}
