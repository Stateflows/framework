using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces;

public interface IStateflowsSubscriber
{
    Task PublishAsync<TNotification>(TNotification notificationEvent, StateflowsContext senderContext, IDictionary<string, EventHeader> headers = null);
    Task SubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
    Task UnsubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
}