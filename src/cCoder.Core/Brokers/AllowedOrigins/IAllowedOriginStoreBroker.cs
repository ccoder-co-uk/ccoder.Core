// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Generic;
namespace cCoder.Core.Brokers.AllowedOrigins;

internal interface IAllowedOriginStoreBroker
{
    IEnumerable<string> GetAllowedOrigins();
}