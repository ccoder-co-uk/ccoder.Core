// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Core.Services.Processings.Formatters;


namespace cCoder.Core.Exposures.Formatters;

internal static class FormatterExportExtensions
{
    public static string ToCsv(
        this object source,
        IEnumerable<Resource> resources,
        string delimiter = ";",
        string quotes = "",
        string culture = ""
    ) =>
        new CsvFileProcessingService(
            resources: resources,
            delimiter: delimiter,
            quotes: quotes,
            culture: culture)
            .BuildCsvFile(source: source);

    public static Stream ToExcel(
        this object source,
        IEnumerable<Resource> resources,
        string culture = ""
    ) =>
        new ExcelFileProcessingService(
            culture: culture,
            resources: resources)
            .BuildExcelFile(data: source);

}