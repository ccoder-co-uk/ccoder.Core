// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.Core.Dependencies.Formatters;

public sealed partial class ExcelFormatter
{
    private static void ValidateExcelFileOnBuild(object data) =>
        ValidationRulesEngine.Validate(inputs: [data]);
}