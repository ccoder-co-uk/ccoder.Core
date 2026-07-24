// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OData;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static class CoreAggregateApiModelBuilderExtensions
{
    internal static void ConfigureCoreAggregateApiModel(this ODataConventionModelBuilder builder) =>
        new CoreAggregateApiModelDependency()
            .Configure(options: builder);
}