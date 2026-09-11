// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.Formatters;

namespace cCoder.Core.Brokers.Formatters;

internal sealed class FormatterODataBroker : IFormatterODataBroker
{
    public object UnpackSelectExpandWrapper(object contextObject) =>
        ODataFormatterDependency.UnpackSelectExpandWrapper(
            contextObject: contextObject);
}