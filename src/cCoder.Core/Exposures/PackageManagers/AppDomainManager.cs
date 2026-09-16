// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Packaging.Exposures.PackageManagers;


namespace cCoder.Core.Exposures.PackageManagers;

internal sealed class AppDomainManager(
    IContentManagementAppService contentManagementAppService) : IAppDomainManager
{
    public string GetDomain(int appId) =>
        contentManagementAppService.GetApp(appId: appId)?.Domain;
}