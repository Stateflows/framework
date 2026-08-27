using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces;

public interface IStateflowsSubscriber
{
    Task PublishRangeAsync<TNotification>(BehaviorId publisherBehaviorId, IEnumerable<TNotification> notificationEvents, StateflowsContext senderContext, IDictionary<string, EventHeader>? headers = null);
    Task SubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
    Task UnsubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
}