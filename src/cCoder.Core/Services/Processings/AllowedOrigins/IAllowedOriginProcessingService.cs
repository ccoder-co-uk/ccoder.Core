// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Processings.AllowedOrigins;

internal interface IAllowedOriginProcessingService
{
    CoreAllowedOriginSnapshot CreateSnapshot(IEnumerable<string> configuredOrigins);

    bool IsAllowed(string origin, CoreAllowedOriginSnapshot snapshot);
}