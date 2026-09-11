// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Metadata;
using cCoder.Core.Models.Metadata;

namespace cCoder.Core.Services.Foundations.Metadata;

internal sealed partial class EdmModelService(IEdmModelBroker edmModelBroker)
    : IEdmModelService
{
    public EdmModelDetails RetrieveEdmModelDetails(
        EdmModelDetails edmModelDetails) =>
        TryCatch(operation: () =>
        {
            ValidateEdmModelDetailsOnRetrieve(
                edmModelDetails: edmModelDetails);

            return edmModelBroker.RetrieveEdmModelDetails(
                edmModelDetails: edmModelDetails);
        });
}