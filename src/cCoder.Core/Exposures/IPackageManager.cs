// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures;

public interface IPackageManager
{
    ValueTask<Package[]> ExportPackagesAsync(
        int appId,
        string[] packageNames,
        string sourceApi);

}