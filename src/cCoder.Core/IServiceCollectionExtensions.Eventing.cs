using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    internal static void AddCoreApiEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders
    )
    {
        services.AddEventing();
        AddCoreEventingTypes(services);
        services.AddEventProviders((eventProviders ?? []).Where(provider => provider is not null).ToArray());
    }

    internal static void AddCoreHostedEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders
    )
    {
        services.AddEventing();
        AddCoreEventingTypes(services);
        services.AddEventProviders((eventProviders ?? []).Where(provider => provider is not null).ToArray());
    }

    private static void AddCoreEventingTypes(IServiceCollection services)
    {
        services.AddEventingForType<App>();
        services.AddEventingForType<AppCulture>();
        services.AddEventingForType<cCoder.Data.Models.CommonObject>();
        services.AddEventingForType<Component>();
        services.AddEventingForType<Content>();
        services.AddEventingForType<Culture>();
        services.AddEventingForType<Layout>();
        services.AddEventingForType<Page>();
        services.AddEventingForType<PageInfo>();
        services.AddEventingForType<cCoder.Data.Models.Security.PageRole>();
        services.AddEventingForType<Resource>();
        services.AddEventingForType<Script>();
        services.AddEventingForType<Submission>();
        services.AddEventingForType<Template>();
        services.AddEventingForType<cCoder.Data.Models.Packaging.Package>();
        services.AddEventingForType<(int, cCoder.Data.Models.Packaging.Package)>();
        services.AddEventingForType<cCoder.Data.Models.Packaging.PackageItem>();
    }
}
