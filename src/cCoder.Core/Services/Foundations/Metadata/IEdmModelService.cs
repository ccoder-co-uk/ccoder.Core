// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;

namespace cCoder.Core.Services.Foundations.Metadata;

internal interface IEdmModelService
{
    EdmModelDetails RetrieveEdmModelDetails(EdmModelDetails edmModelDetails);
}