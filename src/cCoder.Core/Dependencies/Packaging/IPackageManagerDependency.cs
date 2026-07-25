// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Dependencies.Packaging;

internal interface IPackageManagerDependency
{
    Package ExportPackage(
        int appId,
        string packageName);

    ValueTask ImportPackageAsync(
        int appId,
        Package package);
}