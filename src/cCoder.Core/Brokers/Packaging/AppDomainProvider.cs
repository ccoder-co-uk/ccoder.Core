using cCoder.ContentManagement.Services.Processings;
using cCoder.Packaging.Brokers;


namespace cCoder.Core.Brokers.Packaging;

internal class AppDomainProvider(IAppProcessingService appProcessingService) : IAppDomainProvider
{
    public string GetDomain(int appId) => appProcessingService.GetDomain(appId);
}


