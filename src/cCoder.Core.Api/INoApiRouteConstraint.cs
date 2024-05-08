namespace cCoder.Core.Api;

public class NoApiRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        => httpContext.Request.Path.HasValue && !httpContext.Request.Path.Value.ToLower().Contains("/api/");
}
