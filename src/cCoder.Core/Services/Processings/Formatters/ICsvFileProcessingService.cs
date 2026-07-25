// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Formatters;

internal interface ICsvFileProcessingService
{
    string BuildCsvFile(object source);
}