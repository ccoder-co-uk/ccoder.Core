using EventLibrary.Models;

namespace cCoder.Core.Models;

public sealed class CoreHostedServicesOptions
{
    public IConfiguration Configuration { get; set; }

    public EventProvider[] EventProviders { get; private set; } = [];

    public CoreHostedServicesOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        EventProviders = eventProviders ?? [];
        return this;
    }
}
