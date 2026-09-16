// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Brokers.Metadata;

internal interface IEdmModelBroker
{
    IReadOnlyCollection<EdmModelType> RetrieveTypes(IEdmModel model);
    EdmModelOperations RetrieveOperations(IEdmModel model, Type type);
}