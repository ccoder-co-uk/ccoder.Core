// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;

namespace cCoder.Core.Services.Processings.Formatters;

internal sealed partial class CsvFileProcessingService
{
    private static void ValidateCsvFileOnBuild(object source) =>
        ValidationRulesEngine.Validate(inputs: [source]);
}