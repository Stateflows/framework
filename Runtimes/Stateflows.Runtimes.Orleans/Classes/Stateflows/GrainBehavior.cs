using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stateflows;

public class GrainBehavior :
    IBehavior,
    IAsyncDisposable,
    IBehaviorGrainObserver,
    IUnwatcher,
    ITypedNotificationHandler
{
    private readonly string tenantId;
    private readonly BehaviorId behaviorId;
    private readonly IClusterClient client;
    private readonly ILogger<GrainBehavior> logger;
    private readonly TimeSpan renewalInterval;
    private readonly SemaphoreSlim watchGate = new(1, 1);
    private CancellationTokenSource? renewal;
    private bool disposed;
    private Task? disposal;

    public GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client)
        : this(tenantId, behaviorId, client, NullLogger<GrainBehavior>.Instance, TimeSpan.FromMinutes(4))
    { }

    public GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client, ILogger<GrainBehavior> logger)
        : this(tenantId, behaviorId, client, logger, TimeSpan.FromMinutes(4))
    { }

    internal GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client,
        ILogger<GrainBehavior> logger, TimeSpan renewalInterval)
    {
        this.tenantId = tenantId;
        this.behaviorId = behaviorId;
        this.client = client;
        this.logger = logger;
        this.renewalInterval = renewalInterval;
    }

    private IBehaviorGrainObserver? observerReference = null;
    private IBehaviorGrainObserver ObserverReference
        => observerReference ??= client.CreateObjectReference<IBehaviorGrainObserver>(this);
    
    private TenantBehaviorId? tenantBehaviorId;
    private TenantBehaviorId TenantBehaviorId => tenantBehaviorId ??= new TenantBehaviorId() { TenantId = tenantId, BehaviorId = behaviorId, };
    private string? grainKey;
    private string GrainKey => grainKey ??= StateflowsGrainKeys.ForBehavior(TenantBehaviorId);
    private IBehaviorGrain? behaviorGrain;
    private IBehaviorGrain BehaviorGrain => behaviorGrain ??= client.GetGrain<IBehaviorGrain>(GrainKey);
    private ISubscriptionsGrain? subscriptionsGrain;
    private ISubscriptionsGrain SubscriptionsGrain => subscriptionsGrain ??= client.GetGrain<ISubscriptionsGrain>(GrainKey);
    // private ISignalsGrain? cancellationGrain;
    // private ISignalsGrain SignalsGrain => cancellationGrain ??= client.GetGrain<ISignalsGrain>(GrainKey);
    private sealed record Registration(string[] Names, Func<EventHolder, Task> Handler, DateTime ReplaySince)
    {
        public bool Active { get; set; } = true;
    }
    private readonly Dictionary<string, List<Registration>> handlers = new();
    private readonly Dictionary<IWatcher, Registration> handlersByWatcher = new();

    public void Dispose()
    {
        lock (handlers)
        {
            if (disposed) return;
            disposed = true;
            foreach (var registration in handlersByWatcher.Values) registration.Active = false;
            handlers.Clear();
            handlersByWatcher.Clear();
            StopRenewal();
            disposal = Task.Run(CleanupAsync);
            _ = ObserveDisposalAsync(disposal);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        await disposal!;
    }

    private async Task ObserveDisposalAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to dispose notification watches for {BehaviorId}.", Id);
        }
    }

    private async Task CleanupAsync()
    {
        await watchGate.WaitAsync();
        try
        {
            if (observerReference != null)
            {
                try
                {
                    await SubscriptionsGrain.RemoveWatchAsync(observerReference);
                }
                finally
                {
                    DeleteObserverReference();
                }
            }
        }
        finally
        {
            watchGate.Release();
        }
    }

    private void DeleteObserverReference()
    {
        if (observerReference == null) return;
        client.DeleteObjectReference<IBehaviorGrainObserver>(observerReference);
        observerReference = null;
    }

    private void StopRenewal()
    {
        renewal?.Cancel();
        renewal = null;
    }

    private void StartRenewal()
    {
        lock (handlers)
        {
            if (disposed || handlers.Count == 0 || renewal != null) return;
            renewal = new CancellationTokenSource();
            var cancellation = renewal;
            _ = Task.Run(() => RenewAsync(cancellation));
        }
    }

    private async Task RenewAsync(CancellationTokenSource cancellation)
    {
        using (cancellation)
        {
            try
            {
                using var timer = new PeriodicTimer(renewalInterval);
                while (await timer.WaitForNextTickAsync(cancellation.Token))
                {
                    await watchGate.WaitAsync(cancellation.Token);
                    try
                    {
                        string[] names;
                        DateTime replaySince;
                        lock (handlers)
                        {
                            if (disposed || cancellation.IsCancellationRequested) return;
                            names = handlers.Keys.ToArray();
                            replaySince = handlersByWatcher.Values.Select(r => r.ReplaySince).DefaultIfEmpty(DateTime.Now).Min();
                        }
                        if (names.Length > 0)
                            await SubscriptionsGrain.AddWatchAsync(ObserverReference, names, replaySince, renewal: true);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Failed to renew notification watches for {BehaviorId}.", Id);
                    }
                    finally
                    {
                        watchGate.Release();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            { }
        }
    }

    public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        // TODO: handle immediate delivery
        // if (@event is Finalize { Mode: FinalizationMode.Immediate })
        // {
        //     await SignalsGrain.EnableSignalAsync(Signals.Cancel);
        // }
        
        var eventHolder = @event.ToTypedEventHolder(headers);
        var result = await BehaviorGrain.ProcessEventAsync(eventHolder, CancellationToken.None);
        return new SendResult(result.Status, result.Validation);
    }

    public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null)
    {
        var h = (OrleansEventHolder)request.ToEventHolder(headers);
        try
        {
            var result = await BehaviorGrain.ProcessEventAsync(h, CancellationToken.None);
            return new RequestResult<TResponseEvent>((RequestResult)result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
        => (await GetNotificationsAsync([Event<TNotification>.Name], lastNotificationsCheck))
            .Cast<EventHolder<TNotification>>()
            .Select(holder => holder.Payload)
            .ToArray();

    public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
    {
        ArgumentNullException.ThrowIfNull(notificationNames);
        return (await SubscriptionsGrain.GetNotificationsAsync(lastNotificationsCheck, notificationNames))
            .Select(holder => (EventHolder)holder)
            .ToArray();
    }

    public Task<IWatcher> WatchAsync<TNotification>(Func<TNotification, Task> asyncHandler, DateTime? replayNotificationsSince = null)
    {
        ArgumentNullException.ThrowIfNull(asyncHandler);
        return WatchAsync([Event<TNotification>.Name],
            eventHolder => asyncHandler(((EventHolder<TNotification>)eventHolder).Payload), replayNotificationsSince);
    }

    public async Task<IWatcher> WatchAsync(string[] notificationNames, Func<EventHolder, Task> asyncHandler, DateTime? replayNotificationsSince = null)
    {
        ArgumentNullException.ThrowIfNull(notificationNames);
        ArgumentNullException.ThrowIfNull(asyncHandler);
        if (notificationNames.Length == 0 || notificationNames.Any(string.IsNullOrEmpty))
            throw new ArgumentException("At least one nonempty notification name is required.", nameof(notificationNames));

        await watchGate.WaitAsync();
        var watcher = new Watcher(this);
        var registration = new Registration(notificationNames.Distinct().ToArray(), asyncHandler, replayNotificationsSince ?? DateTime.Now);
        try
        {
            lock (handlers)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                foreach (var name in registration.Names)
                {
                    if (!handlers.TryGetValue(name, out var notificationHandlers))
                    {
                        notificationHandlers = [];
                        handlers.Add(name, notificationHandlers);
                    }
                    notificationHandlers.Add(registration);
                }
                handlersByWatcher.Add(watcher, registration);
            }

            try
            {
                await SubscriptionsGrain.AddWatchAsync(ObserverReference, registration.Names, replayNotificationsSince);
                lock (handlers)
                {
                    ObjectDisposedException.ThrowIf(disposed, this);
                }
                StartRenewal();
                return watcher;
            }
            catch
            {
                var removed = RemoveLocalWatch(watcher);
                try
                {
                    if (observerReference != null)
                    {
                        bool removeAll;
                        lock (handlers)
                        {
                            removeAll = handlers.Count == 0;
                        }
                        await SubscriptionsGrain.RemoveWatchAsync(observerReference, removeAll ? null : removed);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to clean up notification watch registration for {BehaviorId}.", Id);
                }
                ReleaseObserverIfUnused();
                throw;
            }
        }
        finally
        {
            watchGate.Release();
        }
    }

    public BehaviorId Id => behaviorId;
    
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
                    foreach (var registration in notificationHandlers)
                    {
                        _ = Task.Run(() => InvokeHandlerAsync(registration, eventHolder));
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private async Task InvokeHandlerAsync(Registration registration, EventHolder notification)
    {
        try
        {
            lock (handlers)
            {
                if (disposed || !registration.Active) return;
            }
            await registration.Handler(notification);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Notification handler failed for {BehaviorId}, notification {NotificationName}.",
                Id, notification.Name);
        }
    }

    private string[] RemoveLocalWatch(IWatcher watcher)
    {
        lock (handlers)
        {
            if (!handlersByWatcher.Remove(watcher, out var registration)) return [];
            registration.Active = false;
            var removed = new List<string>();
            foreach (var name in registration.Names)
            {
                var notificationHandlers = handlers[name];
                notificationHandlers.RemoveAll(r => ReferenceEquals(r, registration));
                if (notificationHandlers.Count == 0)
                {
                    handlers.Remove(name);
                    removed.Add(name);
                }
            }
            if (handlers.Count == 0) StopRenewal();
            return removed.ToArray();
        }
    }

    private void ReleaseObserverIfUnused()
    {
        lock (handlers)
        {
            if (handlers.Count == 0) DeleteObserverReference();
        }
    }

    public async Task UnwatchAsync(IWatcher watcher)
    {
        await watchGate.WaitAsync();
        try
        {
            lock (handlers)
            {
                if (disposed) return;
            }
            var removed = RemoveLocalWatch(watcher);
            try
            {
                if (observerReference != null && removed.Length > 0)
                    await SubscriptionsGrain.RemoveWatchAsync(observerReference, removed);
            }
            finally
            {
                ReleaseObserverIfUnused();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to remove notification watch for {BehaviorId}.", Id);
            throw;
        }
        finally
        {
            watchGate.Release();
        }
    }
}