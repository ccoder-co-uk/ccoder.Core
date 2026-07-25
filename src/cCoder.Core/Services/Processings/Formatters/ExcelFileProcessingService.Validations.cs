// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class ExcelFileProcessingService
{
    private static void ValidateExcelFileOnBuild(object data) =>
        ValidationRulesEngine.Validate(inputs: [data]);
}