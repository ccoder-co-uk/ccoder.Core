// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Exposures;

public class NoApiRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext httpContext,
        IRouter route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection
    ) =>
        httpContext.Request.Path.HasValue
        && !httpContext.Request.Path.Value.ToLower()
            .Contains(value: "/api/");
}