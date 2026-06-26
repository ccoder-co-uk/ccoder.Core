namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal interface IAllowedOriginStoreService
{
    ValueTask<string[]> GetAllowedOriginsAsync();
}
