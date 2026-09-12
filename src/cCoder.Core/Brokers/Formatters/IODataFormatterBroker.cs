// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Formatters;

internal interface IFormatterODataBroker
{
    object UnpackSelectExpandWrapper(object contextObject);
}