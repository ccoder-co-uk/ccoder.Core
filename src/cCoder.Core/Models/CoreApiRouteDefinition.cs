// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.OData.Edm;

namespace cCoder.Core.Models;

internal sealed class CoreApiRouteDefinition
{
    public string Name { get; set; }
    public string RoutePath { get; set; }
    public IEdmModel RouteModel { get; set; }
}