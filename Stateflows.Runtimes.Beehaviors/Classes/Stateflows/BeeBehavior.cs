using Beehaviors.Context.Enums;
using Beehaviors.Context.Structs;
using Beehaviors.Headers;
using Beehaviors;
using Beehaviors.Registration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows;

internal sealed class BeeBehavior : IBehavior, IUnwatcher, INotificationHandler, IInjectionScope
{
    private readonly string tenantId;
    private readonly BehaviorId id;
    private readonly ILocalBeeLocator beeLocator;
    private readonly INotificationsHub notificationsHub;
    private readonly IServiceProvider serviceProvider;
    private readonly IStateflowsTenantProvider tenantProvider;
    private readonly ITenantAccessor tenantAccessor;
    private readonly Dictionary<string, List<Func<EventHolder, Task>>> handlers = [];
    private readonly Dictionary<IWatcher, Func<EventHolder, Task>> handlersByWatcher = [];
    private IBeeHandle? beeHandle;

    public BeeBehavior(
        string tenantId,
        BehaviorId id,
        ILocalBeeLocator beeLocator,
        INotificationsHub notificationsHub,
        IServiceProvider serviceProvider
    )
    {
        this.tenantId = tenantId;
        this.id = id;
        this.beeLocator = beeLocator;
        this.notificationsHub = notificationsHub;
        this.serviceProvider = serviceProvider;

        tenantProvider = serviceProvider.GetRequiredService<IStateflowsTenantProvider>();
        tenantAccessor = serviceProvider.GetRequiredService<ITenantAccessor>();
        notificationsHub.RegisterHandler(this);
    }

    public IServiceProvider ServiceProvider => serviceProvider;

    public BehaviorId Id => id;

    private IBeeHandle BeeHandle
    {
        get
        {
            if (beeHandle is not null)
            {
                return beeHandle;
            }

            var runtimeBeeId = new BeeId(
                StateflowsRuntimeBee.BeeName,
                StateflowsJsonConverter.SerializeObject(new TenantBehaviorId
                {
                    TenantId = tenantId,
                    BehaviorId = id
                })
            );

            if (!beeLocator.TryLocateBee(runtimeBeeId, out var handle))
            {
                throw new InvalidOperationException($"Could not locate runtime bee '{runtimeBeeId}'.");
            }

            beeHandle = handle;
            return beeHandle;
        }
    }

    public async Task HandleNotificationAsync(EventHolder eventHolder)
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

        await Task.CompletedTask;
    }

    private static Dictionary<string, object> ToBeeHeaders(string tenantId, EventHolder eventHolder)
    {
        var headers = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [TenantId.HeaderName] = tenantId,
            [MessageId.HeaderName] = eventHolder.Id
        };

        foreach (var header in eventHolder.Headers)
        {
            headers[header.Key] = header.Value;
        }

        return headers;
    }

    private sealed class BridgeExecutionResult
    {
        public EventStatus Status { get; init; }

        public EventValidation Validation { get; init; } = new(true);

        public Dictionary<object, EventHolder> Responses { get; init; } = [];
    }

    private async Task<BridgeExecutionResult> ProcessEventAsync(EventHolder eventHolder)
    {
        var request = new ProcessBehaviorEventRequest(tenantId, Id, eventHolder);
        var (status, response) = await BeeHandle.RequestAsync(request, ToBeeHeaders(tenantId, eventHolder));

        if (status == MessageStatus.Processed && response is not null)
        {
            var responses = new Dictionary<object, EventHolder>();
            if (response.Response is not null && eventHolder.IsRequest())
            {
                responses[eventHolder.BoxedPayload] = response.Response;
            }

            return new BridgeExecutionResult
            {
                Status = response.Status,
                Validation = response.Validation,
                Responses = responses
            };
        }

        var mappedStatus = status switch
        {
            MessageStatus.Rejected => EventStatus.Rejected,
            MessageStatus.Invalid => EventStatus.Invalid,
            MessageStatus.Failed => EventStatus.Failed,
            _ => EventStatus.Undelivered
        };

        return new BridgeExecutionResult
        {
            Status = mappedStatus,
            Validation = new EventValidation(mappedStatus != EventStatus.Invalid)
        };
    }

    public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        var eventHolder = @event.ToTypedEventHolder(headers);
        var executionResult = await ProcessEventAsync(eventHolder);

        if (ResponseHolder.ResponsesAreSet())
        {
            ResponseHolder.CopyResponses(executionResult.Responses);
        }

        return new SendResult(executionResult.Status, executionResult.Validation);
    }

    public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader>? headers = null)
    {
        var eventHolder = request.ToTypedEventHolder(headers);
        var executionResult = await ProcessEventAsync(eventHolder);

        if (ResponseHolder.ResponsesAreSet())
        {
            ResponseHolder.CopyResponses(executionResult.Responses);
        }
        else
        {
            ResponseHolder.SetResponses(executionResult.Responses);
        }

        return new RequestResult<TResponse>(eventHolder, executionResult.Status, executionResult.Validation);
    }

    public async Task<IWatcher> WatchAsync<TNotification>(Func<TNotification, Task> asyncHandler, DateTime? replayNotificationsSince = null)
    {
        var watcher = new Watcher(this);
        lock (handlers)
        {
            var notificationName = typeof(TNotification).GetEventName();
            if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
            {
                notificationHandlers = [];
                handlers.Add(notificationName, notificationHandlers);
            }

            Func<EventHolder, Task> notificationHandler = eventHolder => asyncHandler(((EventHolder<TNotification>)eventHolder).Payload);
            notificationHandlers.Add(notificationHandler);
            handlersByWatcher[watcher] = notificationHandler;
        }

        tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
        var lastNotificationCheck = replayNotificationsSince ?? DateTime.Now;
        var pendingNotifications = await notificationsHub.GetNotificationsAsync<TNotification>(Id, lastNotificationCheck);
        foreach (var pendingNotification in pendingNotifications)
        {
            asyncHandler(pendingNotification);
        }

        return watcher;
    }

    public async Task<IWatcher> WatchAsync(string[] notificationNames, Func<EventHolder, Task> asyncHandler, DateTime? replayNotificationsSince = null)
    {
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

                notificationHandlers.Add(asyncHandler);
            }

            handlersByWatcher[watcher] = asyncHandler;
        }

        tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
        var lastNotificationCheck = replayNotificationsSince ?? DateTime.Now;
        var pendingNotifications = await notificationsHub.GetNotificationsAsync(Id, notificationNames, lastNotificationCheck);
        foreach (var pendingNotification in pendingNotifications)
        {
            asyncHandler(pendingNotification);
        }

        return watcher;
    }

    public Task UnwatchAsync(IWatcher watcher)
    {
        lock (handlers)
        {
            if (handlersByWatcher.TryGetValue(watcher, out var handler))
            {
                foreach (var handlersList in handlers.Values)
                {
                    handlersList.Remove(handler);
                }

                foreach (var emptyItem in handlers.Where(h => !h.Value.Any()).ToArray())
                {
                    handlers.Remove(emptyItem.Key);
                }

                handlersByWatcher.Remove(watcher);
            }
        }

        return Task.CompletedTask;
    }

    [Obsolete("Use WatchStatusAsync instead.")]
    public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
    {
        tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
        return await notificationsHub.GetNotificationsAsync<TNotification>(Id, lastNotificationsCheck);
    }

    [Obsolete("Use WatchStatusAsync instead.")]
    public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
    {
        tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
        return await notificationsHub.GetNotificationsAsync(Id, notificationNames, lastNotificationsCheck);
    }

    public void Dispose()
    {
        notificationsHub.UnregisterHandler(this);
        GC.SuppressFinalize(this);
    }
}
