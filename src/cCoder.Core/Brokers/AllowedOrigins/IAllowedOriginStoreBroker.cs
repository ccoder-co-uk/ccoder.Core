// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.AllowedOrigins;

internal interface IAllowedOriginStoreBroker
{
    IEnumerable<string> GetAllowedOrigins();
}