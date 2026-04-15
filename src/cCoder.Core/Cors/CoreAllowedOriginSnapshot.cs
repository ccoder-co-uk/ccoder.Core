namespace cCoder.Core.Cors;

public sealed record CoreAllowedOriginSnapshot(
    IReadOnlySet<string> ExactOrigins,
    IReadOnlySet<string> Authorities,
    IReadOnlySet<string> Hosts);
