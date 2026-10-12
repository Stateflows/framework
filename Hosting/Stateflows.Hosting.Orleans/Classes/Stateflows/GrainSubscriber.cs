using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

public class GrainSubscriber(IClusterClient client, IStateflowsTenantProvider tenantProvider) : IStateflowsSubscriber
{
    private async Task<ISubscriptionsGrain> GetSubscriptionsGrain(BehaviorId behaviorId)
    {
        var tenantId = await tenantProvider.GetCurrentTenantIdAsync();
        var id = StateflowsJsonConverter.SerializeObject(
            new TenantBehaviorId()
            {
                TenantId = tenantId ?? "host",
                BehaviorId = behaviorId
            }
        );
        var notificationsGrain = client.GetGrain<ISubscriptionsGrain>(id);
        return notificationsGrain;
    }

    public async Task PublishRangeAsync<TNotification>(BehaviorId publisherBehaviorId, IEnumerable<TNotification> notificationEvents,
        StateflowsContext senderContext, IDictionary<string, EventHeader>? headers = null)
    {
        if (!notificationEvents.Any())
        {
            return;
        }
        
        var notificationType = typeof(TNotification);
        var ttlAttribute = notificationType.GetCustomAttribute<TimeToLiveAttribute>();
        var retainAttribute = notificationType.GetCustomAttribute<RetainAttribute>();

        var orleansEventHolders = notificationEvents
            .Select(n =>
            {
                var notification = (OrleansEventHolder)n.ToEventHolder(headers, senderContext.Id);
                notification.SentAt = DateTime.Now;
                notification.Retained = retainAttribute != null;
                notification.TimeToLive = ttlAttribute?.SecondsToLive ?? 0;
                return notification;
            })
            .ToArray();
        
        var notificationsGrain = await GetSubscriptionsGrain(senderContext.Id);
        _ = notificationsGrain.PublishAsync(orleansEventHolders);
    }

    public async Task SubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId)
    {
        var notificationsGrain = await GetSubscriptionsGrain(subscribedBehaviorId);
        await notificationsGrain.AddSubscriptionAsync(subscriberBehaviorId, [Event<TNotification>.Name]);
    }

    public async Task UnsubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId)
    {
        var notificationsGrain = await GetSubscriptionsGrain(subscribedBehaviorId);
        await notificationsGrain.RemoveSubscriptionAsync(subscriberBehaviorId, [Event<TNotification>.Name]);
    }
}