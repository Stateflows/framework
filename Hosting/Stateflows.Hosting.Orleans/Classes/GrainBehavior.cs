using Orleans.Serialization.Serializers;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

public class GrainBehavior(string tenantId, BehaviorId behaviorId, IClusterClient client) : IBehavior
{
    private IBehaviorGrain? grain = null;

    private IBehaviorGrain Grain
    {
        get
        {
            if (grain == null)
            {
                var tenantBehaviorId = new TenantBehaviorId()
                {
                    TenantId = tenantId,
                    BehaviorId = behaviorId,
                };
                
                grain = client.GetGrain<IBehaviorGrain>(StateflowsJsonConverter.SerializeObject(tenantBehaviorId));
            }
            
            return grain;
        }
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(@event.ToEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        
        var result = await Grain.ProcessEventAsync(@event.ToEventHolder(headers));
        return new SendResult(result.Status, result.Validation);
    }

    public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null)
    {
        // var serializedEventHolder = StateflowsJsonConverter.SerializePolymorphicObject(request.ToTypedEventHolder(headers));
        // var serializedResult = await Grain.ProcessAsync(serializedEventHolder);
        // var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(serializedResult);
        
        var result = await Grain.ProcessEventAsync(request.ToEventHolder(headers));
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
    
    public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
    {
        return Task.FromResult<IEnumerable<TNotification>>(Array.Empty<TNotification>());
    }

    public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
    {
        return Task.FromResult<IEnumerable<EventHolder>>(Array.Empty<EventHolder>());
    }
}