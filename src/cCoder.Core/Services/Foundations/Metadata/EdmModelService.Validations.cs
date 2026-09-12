// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Core.Models.Metadata;

namespace cCoder.Core.Services.Foundations.Metadata;

internal sealed partial class EdmModelService
{
    private static void ValidateEdmModelDetailsOnRetrieve(
        EdmModelDetails edmModelDetails) =>
        ValidationRulesEngine.Validate(inputs: [edmModelDetails]);
}