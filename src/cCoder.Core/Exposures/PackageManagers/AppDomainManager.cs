// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Packaging.Exposures.PackageManagers;


namespace cCoder.Core.Exposures.PackageManagers;

internal sealed class AppDomainManager(IAppManager appManager) : IAppDomainManager
{
    public string GetDomain(int appId) =>
        appManager.Get(appManagerId: appId)?.Domain;
}