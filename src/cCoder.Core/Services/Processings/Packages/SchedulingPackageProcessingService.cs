// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Packages;
using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Services.Processings.Packages;

internal sealed partial class SchedulingPackageProcessingService(
    ISchedulingPackageService schedulingPackageService)
    : ISchedulingPackageProcessingService
{
    public ValueTask ImportPackageAsync(int appId, Package package) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnImport(appId: appId, package: package);
            return schedulingPackageService.ImportPackageAsync(appId: appId, package: package);
        });

    public Package ExportPackage(int appId, string packageName) =>
        TryCatch(operation: () =>
        {
            ValidatePackageOnExport(appId: appId, packageName: packageName);
            return schedulingPackageService.ExportPackage(appId: appId, packageName: packageName);
        });
}