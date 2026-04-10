using Microsoft.AspNetCore.Http.Features;


namespace cCoder.Core.Api;

public sealed class NoOpSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = NoOpSession.Instance;
}

