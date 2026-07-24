// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.OData.Edm;

namespace cCoder.Core.Models.OData;

public class ODataModelContract
{
    public string Context { get; set; }
    public string Description { get; set; }
    public IEdmModel EDMModel { get; set; }
}
