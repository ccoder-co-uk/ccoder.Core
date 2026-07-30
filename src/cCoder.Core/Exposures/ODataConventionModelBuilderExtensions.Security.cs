// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OData;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static partial class ODataConventionModelBuilderExtensions
{
    internal static void ConfigureCoreSecurityApiModel(this ODataConventionModelBuilder builder) =>
        new CoreSecurityApiModelDependency()
            .Configure(options: builder);
}