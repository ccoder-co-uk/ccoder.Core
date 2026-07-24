// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http.Features;


namespace cCoder.Core.Exposures;

public sealed class NoOpSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = NoOpSession.Instance;
}