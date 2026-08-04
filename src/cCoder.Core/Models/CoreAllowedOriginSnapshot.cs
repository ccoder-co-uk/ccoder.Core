// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class CoreAllowedOriginSnapshot
{
    public IReadOnlySet<string> ExactOrigins { get; set; }
    public IReadOnlySet<string> Authorities { get; set; }
    public IReadOnlySet<string> Hosts { get; set; }
}