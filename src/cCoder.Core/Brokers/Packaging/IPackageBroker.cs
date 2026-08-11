// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Brokers.Packaging;

internal interface IPackageBroker
{
    Package ExportPackage(
        int appId,
        string packageName);

}