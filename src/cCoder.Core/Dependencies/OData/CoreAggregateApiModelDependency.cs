// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Dependencies.OData;

internal sealed class CoreAggregateApiModelDependency
    : IConfigureOptions<ODataConventionModelBuilder>
{
    public void Configure(ODataConventionModelBuilder options)
    {
        _ = options.EntitySet<Package>(name: "Package");
        _ = options.EntitySet<PackageItem>(name: "PackageItem");
    }
}