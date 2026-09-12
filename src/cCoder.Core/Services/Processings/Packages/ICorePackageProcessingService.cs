// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal interface ICorePackageProcessingService
{
    ValueTask ImportPackageAsync(int appId, Package package);
    ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi);
    ValueTask<Package> ExportPageRolesAsync(int appId, string sourceApi);
    ValueTask<Package> ExportFolderRolesAsync(int appId, string sourceApi);
    Package ExportPackage(int appId, string packageName);
}