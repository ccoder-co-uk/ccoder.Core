// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using Microsoft.AspNetCore.Mvc;


namespace cCoder.Core.Exposures.OData;

public static class ControllerReadExtensions
{
    public static IActionResult ResolveKeyedGet<TEntity>(
        this ControllerBase controller,
        Func<TEntity> get
    )
        where TEntity : class
    {
        try
        {
            TEntity entity = get();
            return entity is null ? controller.NotFound() : controller.Ok(value: entity);
        }
        catch (SecurityException)
        {
            return controller.NotFound();
        }
    }
}