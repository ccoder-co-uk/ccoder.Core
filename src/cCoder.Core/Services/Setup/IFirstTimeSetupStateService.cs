namespace cCoder.Core.Services.Setup;

public interface IFirstTimeSetupStateService
{
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
}
