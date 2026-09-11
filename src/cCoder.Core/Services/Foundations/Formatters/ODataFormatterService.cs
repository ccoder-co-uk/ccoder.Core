// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Formatters;

namespace cCoder.Core.Services.Foundations.Formatters;

internal sealed partial class FormatterODataService(
    IFormatterODataBroker formatterODataBroker)
    : IFormatterODataService
{
    public object UnpackSelectExpandWrapper(object contextObject) =>
        TryCatch(operation: () =>
        {
            ValidateContextObjectOnUnpack(contextObject: contextObject);

            return formatterODataBroker.UnpackSelectExpandWrapper(
                contextObject: contextObject);
        });
}