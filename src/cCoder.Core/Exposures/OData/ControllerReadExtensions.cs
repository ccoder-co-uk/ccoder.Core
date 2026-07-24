// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OData;
using Microsoft.AspNetCore.Mvc;


namespace cCoder.Core.Exposures.OData;

public static class ControllerReadExtensions
{
    public static IActionResult ResolveKeyedGet<TEntity>(
        this ControllerBase controller,
        Func<TEntity> get)
        where TEntity : class =>
        new ControllerReadDependency()
            .Resolve(controller: controller, get: get);
}