using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    internal static void AddCoreEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders
    )
    {
        services.AddEventing(configuration =>
        {
            configuration.EventProviders =
                (eventProviders ?? []).Where(provider => provider is not null).ToArray();
        });
    }
}
