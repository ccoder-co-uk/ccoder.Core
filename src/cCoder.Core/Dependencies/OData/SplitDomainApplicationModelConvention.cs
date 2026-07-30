// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace cCoder.Core.Dependencies.OData;

internal sealed class SplitDomainApplicationModelConvention
    : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        if (!string.Equals(
                a: action.Controller.ControllerName,
                b: "App",
                comparisonType: StringComparison.Ordinal))
        {
            return;
        }

        for (int index = action.Selectors.Count - 1; index >= 0; index--)
        {
            string template =
                action.Selectors[index].AttributeRouteModel?.Template;

            if (template?.StartsWith(
                    value: "Api/Core/App",
                    comparisonType:
                        StringComparison.OrdinalIgnoreCase) == true)
            {
                action.Selectors.RemoveAt(index: index);
            }
        }
    }
}