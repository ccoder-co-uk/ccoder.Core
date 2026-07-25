// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Formatters;

internal interface IExcelFileProcessingService
{
    Stream BuildExcelFile(object data);
}