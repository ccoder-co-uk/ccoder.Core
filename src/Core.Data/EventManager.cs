using Core.Objects;
using Core.Objects.Entities.Workflow;
using Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core;

public abstract class EventManager
{
    protected IEnumerable<WorkflowEvent> Subscriptions { get; set; }

    protected string DataContext { get; }

    protected ICoreAuthInfo Auth { get; }

    protected Config Config { get; }

    protected ILogger Log { get; }

    protected ICoreDataContext Core { get; set; }

    protected IDictionary<string, ICollection<object>> RaisedEvents { get; }

    protected EventManager(ILogger log, ICoreDataContext core, Config config, ICoreAuthInfo auth, string dataContext)
    {
        Log = log;
        Core = core;
        DataContext = dataContext;
        Config = config;
        RaisedEvents = new Dictionary<string, ICollection<object>>();

        if (core != null && config != null && auth != null && dataContext != "Core")
        {
            core.DisableFilters();

            Subscriptions = core.GetAll<WorkflowEvent>(false)
                .Include(sub => sub.Flow)
                .Include(sub => sub.ExecuteAsUser)
                    .ThenInclude(u => u.Roles)
                        .ThenInclude(r => r.Role)
                .Where(e => e.Type.StartsWith(dataContext))
                .ToArray();

            core.EnableFilters();
        }

        Subscriptions ??= Array.Empty<WorkflowEvent>();
    }

    public virtual async Task RaiseEvent<T>(T forObject, string name)
        where T : class
    {
        Log.LogDebug($"Core Event: {name}");

        string eventType = $"{DataContext}/{typeof(T).Name}";

        WorkflowEvent[] subs = Subscriptions
            .Where(s => s.Type == eventType && s.EventContext == name)
            .ToArray();

        if (subs.Length != 0)
        {
            Log.LogDebug($"Found: {subs.Length} subscribers, calling ...");

            IEnumerable<Task> workload = subs
                .Select(s => QueueHandlingFlowInstanceSafely(s, forObject));
            
            await Task.WhenAll(workload);
        }
    }

    public virtual async Task RaiseEvents(object[] forObjects, string name) => 
        await Task.WhenAll(forObjects.Select(o => RaiseEvent(o, name)));

    async Task QueueHandlingFlowInstanceSafely<T>(WorkflowEvent eventSub, T source)
    {
        try
        {
            await eventSub.Flow.QueueNewInstance(Core, eventSub.ExecuteAsUser, source.ToJson(1));
        }
        catch (Exception ex) 
        {
            Log.LogWarning($"Exception thrown whilst raising event for object of type {typeof(T).Name}:\n{ex.Message}\n{ex.StackTrace}");
            Log.LogWarning($"Failed to queue new instance handle for subscription:\n\tSubscriptionId: {eventSub.Id}\n\tFlowId: {eventSub.FlowId}\n\tSource:\n {source.ToJson(1)}");
        }
    }
}