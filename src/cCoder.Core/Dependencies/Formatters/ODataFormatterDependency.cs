// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace cCoder.Core.Dependencies.Formatters;

internal static class ODataFormatterDependency
{
    internal static object UnpackSelectExpandWrapper(object contextObject) =>
        contextObject is ISelectExpandWrapper wrapper
            ? wrapper.ToDictionary()
            : contextObject;
}