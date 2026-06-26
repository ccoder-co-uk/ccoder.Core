namespace cCoder.Core.Models;

public sealed record CoreAllowedOriginSnapshot(
    IReadOnlySet<string> ExactOrigins,
    IReadOnlySet<string> Authorities,
    IReadOnlySet<string> Hosts);
