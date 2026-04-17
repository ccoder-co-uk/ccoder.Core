namespace Web.Services.Setup;

public interface IFirstTimeSetupStateService
{
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
}
