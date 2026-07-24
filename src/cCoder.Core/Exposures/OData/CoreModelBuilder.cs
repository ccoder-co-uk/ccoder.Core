// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OData;
using cCoder.Core.Models;

namespace cCoder.Core.Exposures.OData;

public sealed class CoreModelBuilder : ODataModelBuilder
{
    public override ODataModel Build() =>
        new CoreModelBuilderDependency()
            .Build();
}