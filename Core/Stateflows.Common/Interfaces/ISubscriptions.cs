using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public interface ISubscriptions
    {
        Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new();

        Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new();
    }
}