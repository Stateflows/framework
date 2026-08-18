using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

public class GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client) :
    IBehavior,
    IBehaviorGrainObserver,
    IUnwatcher,
    ITypedNotificationHandler
{
    private System.Timers.Timer? Timer;

    private void SetupTimer()
    {
        if (Timer == null)
        {
            Timer = new System.Timers.Timer(TimeSpan.FromMinutes(4)) { AutoReset = false };
            Timer.Elapsed += async (_, _) =>
            {
                string[] notificationNames;
                lock (handlers)
                {
                    notificationNames = handlers.Keys.Distinct().ToArray();
                }
                
                await SubscriptionsGrain.AddWatchAsync(ObserverReference, notificationNames);
            };
        }

        var enabled = false;
        lock (handlers)
        {
            enabled = handlers.Any();
        }

        Timer.Enabled = enabled;
    }

    private IBehaviorGrainObserver? observerReference = null;
    private IBehaviorGrainObserver ObserverReference
        => observerReference ?? client.CreateObjectReference<IBehaviorGrainObserver>(this);
    
    private TenantBehaviorId? tenantBehaviorId;
    private TenantBehaviorId TenantBehaviorId => tenantBehaviorId ??= new TenantBehaviorId() { TenantId = tenantId, BehaviorId = behaviorId, };
    private string? grainKey;
    private string GrainKey => grainKey ??= StateflowsJsonConverter.SerializeObject(TenantBehaviorId);
    private IBehaviorGrain? behaviorGrain;
    private IBehaviorGrain BehaviorGrain => behaviorGrain ??= client.GetGrain<IBehaviorGrain>(GrainKey);
    private ISubscriptionsGrain? subscriptionsGrain;
    private ISubscriptionsGrain SubscriptionsGrain => subscriptionsGrain ??= client.GetGrain<ISubscriptionsGrain>(GrainKey);
    private ISignalsGrain? cancellationGrain;
    private ISignalsGrain SignalsGrain => cancellationGrain ??= client.GetGrain<ISignalsGrain>(GrainKey);
    private readonly Dictionary<string, List<Action<EventHolder>>> handlers = new();
    private readonly Dictionary<IWatcher, Action<EventHolder>> handlersByWatcher = new();

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(@event.ToEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        if (@event is Finalize { Mode: FinalizationMode.Immediate })
        {
            await SignalsGrain.EnableSignalAsync(Signals.Cancel);
        }
        
        var result = await BehaviorGrain.ProcessEventAsync(@event.ToEventHolder(headers), CancellationToken.None);
        return new SendResult(result.Status, result.Validation);
    }

    public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(request.ToTypedEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        
        var result = await BehaviorGrain.ProcessEventAsync(request.ToEventHolder(headers), CancellationToken.None);
        var response = ((EventHolder?)result.Response) is EventHolder<TResponseEvent> responseEventHolder
            ? responseEventHolder
            : default;
        
        return new RequestResult<TResponseEvent>(response, result.Status, result.Validation);
    }

    public async Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificationsSince = null)
    {
        await SubscriptionsGrain.AddWatchAsync(ObserverReference, [Event<TNotification>.Name]);
        
        var watcher = new Watcher(this);
        lock (handlers)
        {
            var notificationName = typeof(TNotification).GetEventName();
            if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
            {
                notificationHandlers = [];
                handlers.Add(notificationName, notificationHandlers);
            }

            Action<EventHolder> notificationHandler = eventHolder => handler(((EventHolder<TNotification>)eventHolder).Payload);
            notificationHandlers.Add(notificationHandler);
            handlersByWatcher[watcher] = notificationHandler;
        }

        SetupTimer();

        return watcher;
    }

    public async Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificationsSince = null)
    {
        await SubscriptionsGrain.AddWatchAsync(ObserverReference, notificationNames);
        
        var watcher = new Watcher(this);
        lock (handlers)
        {
            foreach (var notificationName in notificationNames)
            {
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = [];
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(handler);
                handlersByWatcher[watcher] = handler;
            }
        }

        SetupTimer();

        return watcher;
    }

    public BehaviorId Id => behaviorId;
    
    // public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
    //     => (await SubscriptionsGrain.GetNotificationsAsync(lastNotificationsCheck, [Event<TNotification>.Name]))
    //         .Select(h => StateflowsJsonConverter.DeserializeObject<TNotification>(h.Payload))
    //         .ToArray();
    //
    // public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
    //     => (await SubscriptionsGrain.GetNotificationsAsync(lastNotificationsCheck, notificationNames))
    //         .Select(h => (EventHolder)h)
    //         .ToArray();

    public async Task NotifyAsync(OrleansEventHolder[] notifications)
    {
        foreach (var notification in notifications)
        {
            EventHolder eventHolder = notification;
            await eventHolder.NotifyAsync(this);
        }
    }

    public Task HandleNotificationAsync<T>(EventHolder<T> eventHolder)
    {
        if (eventHolder.SenderId == Id)
        {
            lock (handlers)
            {
                if (handlers.TryGetValue(eventHolder.Name, out var notificationHandlers))
                {
                    foreach (var handler in notificationHandlers)
                    {
                        handler.Invoke(eventHolder);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    public async Task UnwatchAsync(IWatcher watcher)
    {
        var notificationNames = new List<string>();
        lock (handlers)
        {
            if (handlersByWatcher.TryGetValue(watcher, out var handler))
            {
                foreach (var handlersList in handlers.Values)
                {
                    handlersList.Remove(handler);
                }
                
                foreach (var emptyItem in handlers.Where(h => !h.Value.Any()))
                {
                    handlers.Remove(emptyItem.Key);
                    notificationNames.Add(emptyItem.Key);
                }

                handlersByWatcher.Remove(watcher);
            }
        }
        
        await SubscriptionsGrain.RemoveWatchAsync(ObserverReference, notificationNames.ToArray());
    }
}