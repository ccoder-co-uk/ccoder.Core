using EventLibrary.Models;

namespace cCoder.Core.Models;

public sealed class CoreWebOptions
{
    public IConfiguration Configuration { get; set; }

    public EventProvider[] EventProviders { get; private set; } = [];

    public CoreWebOptions WithEventProviders(params EventProvider[] eventProviders)
    {
        EventProviders = eventProviders ?? [];
        return this;
    }
}
