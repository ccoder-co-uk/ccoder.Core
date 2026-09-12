// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Foundations.Formatters;

internal sealed partial class FormatterODataService
{
    private static void ValidateContextObjectOnUnpack(object contextObject) =>
        ValidationRulesEngine.Validate(inputs: [contextObject]);
}