using Microsoft.OData.Edm;


namespace cCoder.Core;

public static class ContentManagementInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddContentManagementInfrastructure(
        this IServiceCollection services,
        IDictionary<string, IEdmModel> map = null
    ) => services;
}



