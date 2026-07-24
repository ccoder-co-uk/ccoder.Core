// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.OData.Edm;

namespace cCoder.Core.Models;

internal sealed record CoreApiRouteDefinition(
    string Name,
    string RoutePath,
    IEdmModel RouteModel
);