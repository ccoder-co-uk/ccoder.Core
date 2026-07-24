// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OData;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static class CoreSecurityApiModelBuilderExtensions
{
    internal static void ConfigureCoreSecurityApiModel(this ODataConventionModelBuilder builder) =>
        new CoreSecurityApiModelDependency()
            .Configure(options: builder);
}