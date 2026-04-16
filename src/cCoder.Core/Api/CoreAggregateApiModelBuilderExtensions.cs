using cCoder.Data.Models.Packaging;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Api;

internal static class CoreAggregateApiModelBuilderExtensions
{
    internal static void ConfigureCoreAggregateApiModel(this ODataConventionModelBuilder builder)
    {
        _ = builder.EntitySet<Package>("Package");
        _ = builder.EntitySet<PackageItem>("PackageItem");
    }
}
