// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class FormatterODataProcessingService
{
    private static void ValidateContextObjectOnHandle(object contextObject) =>
        ValidationRulesEngine.Validate(inputs: [contextObject]);
}