// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Dependencies.Formatters;

public sealed partial class ExcelFormatter
{
    private static void ValidateExcelFileOnBuild(object data) =>
        ValidationRulesEngine.Validate(inputs: [data]);
}