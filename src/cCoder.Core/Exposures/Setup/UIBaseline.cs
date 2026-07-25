// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures.Setup;

public static partial class UIBaseline
{
    public static Package[] GetPackages() =>
        [
            CreateResourcesPackage(),
            CreatePagesPackage(),
            CreateComponentsPackage()
        ];
}