using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface ISubscriptions
    {
        Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new();

        Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new();
    }
}