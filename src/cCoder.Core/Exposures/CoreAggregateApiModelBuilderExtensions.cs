// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static class CoreAggregateApiModelBuilderExtensions
{
    internal static void ConfigureCoreAggregateApiModel(this ODataConventionModelBuilder builder)
    {
        _ = builder.EntitySet<Package>(name: "Package");
        _ = builder.EntitySet<PackageItem>(name: "PackageItem");
    }
}