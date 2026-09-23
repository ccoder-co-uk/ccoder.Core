// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal interface IContentManagementAppPackageProcessingService
{
    ValueTask ImportPackageAsync(int appId, Package package);
    ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi);
}