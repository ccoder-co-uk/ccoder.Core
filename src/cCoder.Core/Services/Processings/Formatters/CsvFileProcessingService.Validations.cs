// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.Core.Dependencies.Formatters;

public sealed partial class CsvFormatter
{
    private static void ValidateCsvFileOnBuild(object source) =>
        ValidationRulesEngine.Validate(inputs: [source]);
}