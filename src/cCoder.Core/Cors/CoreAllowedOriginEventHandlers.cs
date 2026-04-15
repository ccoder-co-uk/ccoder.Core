using cCoder.Data.Models.CMS;
using EventLibrary;

namespace cCoder.Core.Cors;

internal sealed class CoreAllowedOriginEventHandlers(IEventHub eventHub) : ICoreEventHandlers
{
    private int hasStarted;

    public void ListenToAllEvents()
    {
        if (Interlocked.Exchange(ref hasStarted, 1) == 1)
            return;

        eventHub.ListenToEvent<App, ICoreAllowedOriginStore>(
            "app_add",
            static (store, _) => new ValueTask(store.RefreshAsync()));

        eventHub.ListenToEvent<App, ICoreAllowedOriginStore>(
            "app_update",
            static (store, _) => new ValueTask(store.RefreshAsync()));

        eventHub.ListenToEvent<App, ICoreAllowedOriginStore>(
            "app_delete",
            static (store, _) => new ValueTask(store.RefreshAsync()));
    }
}
