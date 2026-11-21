using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

public class GrainSubscriber(
    IClusterClient client,
    IStateflowsTenantProvider tenantProvider,
    IStateflowsInterceptor stateflowsInterceptor,
    IServiceProvider serviceProvider
) : IStateflowsSubscriber
{
    private async Task<ISubscriptionsGrain> GetSubscriptionsGrain(BehaviorId behaviorId)
    {
        var id = StateflowsGrainKeys.ForBehavior(tenantProvider.GetCurrentTenantId(), behaviorId);
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

        var orleansEventHolders = await Task.WhenAll(notificationEvents
            .Select(async n =>
            {
                var notificationHeaders = headers?.ToDictionary() ?? new Dictionary<string, EventHeader>();

                using var context = new BehaviorActionContext(senderContext, serviceProvider);

                await stateflowsInterceptor.NotificationPublishedAsync(context, n, notificationHeaders);

                var notification = n.ToEventHolder(notificationHeaders, publisherBehaviorId);
                notification.SentAt = DateTime.Now;
                notification.Retained = retainAttribute != null || notificationHeaders.Values.OfType<Retain>().FirstOrDefault() != null;
                notification.TimeToLive = ttlAttribute?.SecondsToLive ?? notificationHeaders.Values.OfType<TimeToLive>().FirstOrDefault()?.SecondsToLive ?? 0;

                return (OrleansEventHolder)notification;
            })
            .ToArray()
        );
        
        var notificationsGrain = await GetSubscriptionsGrain(publisherBehaviorId);
        async Task PublishAsync()
        {
            try
            {
                await notificationsGrain.PublishAsync(orleansEventHolders);
            }
            catch
            {
                StateflowsOrleansDiagnostics.PublicationFailed("publish");
                throw;
            }
        }
        if (!PublicationScope.TryDispatch(PublishAsync))
            await PublishAsync();
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