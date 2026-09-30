// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Generic;
namespace cCoder.Core.Models;

public sealed class CoreAllowedOriginSnapshot
{
    public IReadOnlySet<string> ExactOrigins { get; set; }
    public IReadOnlySet<string> Authorities { get; set; }
    public IReadOnlySet<string> Hosts { get; set; }
}