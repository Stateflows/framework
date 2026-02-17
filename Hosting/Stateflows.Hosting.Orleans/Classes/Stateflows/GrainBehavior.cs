using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

public class GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client) : IBehavior
{
    private TenantBehaviorId? tenantBehaviorId;
    private TenantBehaviorId TenantBehaviorId => tenantBehaviorId ??= new TenantBehaviorId() { TenantId = tenantId, BehaviorId = behaviorId, };
    private string? grainKey;
    private string GrainKey => grainKey ??= StateflowsJsonConverter.SerializeObject(TenantBehaviorId);
    private IBehaviorGrain? behaviorGrain;
    private IBehaviorGrain BehaviorGrain => behaviorGrain ??= client.GetGrain<IBehaviorGrain>(GrainKey);
    private INotificationsGrain? notificationsGrain;
    private INotificationsGrain NotificationsGrain => notificationsGrain ??= client.GetGrain<INotificationsGrain>(GrainKey);
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(@event.ToEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        
        var result = await BehaviorGrain.ProcessEventAsync(@event.ToEventHolder(headers));
        return new SendResult(result.Status, result.Validation);
    }

    public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(request.ToTypedEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        
        var result = await BehaviorGrain.ProcessEventAsync(request.ToEventHolder(headers));
        var response = ((EventHolder?)result.Response) is EventHolder<TResponseEvent> responseEventHolder
            ? responseEventHolder
            : default;
        
        return new RequestResult<TResponseEvent>(response, result.Status, result.Validation);
    }

    public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificatonsSince = null)
    {
        throw new NotImplementedException();
    }

    public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificatonsSince = null)
    {
        throw new NotImplementedException();
    }

    public BehaviorId Id => behaviorId;
    
    public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
        => (await NotificationsGrain.GetNotificationsAsync(lastNotificationsCheck, [Event<TNotification>.Name]))
            .Select(h => StateflowsJsonConverter.DeserializeObject<TNotification>(h.Payload))
            .ToArray();

    public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
        => (await NotificationsGrain.GetNotificationsAsync(lastNotificationsCheck, notificationNames))
            .Select(h => (EventHolder)h)
            .ToArray();
}