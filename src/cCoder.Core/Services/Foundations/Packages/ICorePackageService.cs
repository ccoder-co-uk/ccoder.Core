// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Foundations.Packages;

internal interface ICorePackageService
{
    ValueTask ImportAppConfigurationAsync(int appId, string data);
    ValueTask<Package> ExportAppConfigurationAsync(int appId, string sourceApi);
    ValueTask<Package> ExportPageRolesAsync(int appId, string sourceApi);
    ValueTask<Package> ExportFolderRolesAsync(int appId, string sourceApi);
    Package ExportPackage(int appId, string packageName);
}