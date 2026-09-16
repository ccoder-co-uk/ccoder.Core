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

            if (edmModelDetails.Type is null)
            {
                edmModelDetails.Types = edmModelBroker.RetrieveTypes(
                    model: (Microsoft.OData.Edm.IEdmModel)edmModelDetails.Model);
            }
            else
            {
                edmModelDetails.Operations = edmModelBroker.RetrieveOperations(
                    model: (Microsoft.OData.Edm.IEdmModel)edmModelDetails.Model,
                    type: edmModelDetails.Type);
            }

            return edmModelDetails;
        });
}