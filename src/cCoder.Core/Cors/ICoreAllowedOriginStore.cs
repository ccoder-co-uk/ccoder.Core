namespace cCoder.Core.Cors;

public interface ICoreAllowedOriginStore
{
    CoreAllowedOriginSnapshot Snapshot { get; }
    bool IsAllowed(string origin);
    Task RefreshAsync();
}
