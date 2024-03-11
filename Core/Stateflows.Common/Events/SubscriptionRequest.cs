namespace Stateflows.Common
{
    public sealed class SubscriptionRequest : Request<SubscriptionResponse>
    {
        public BehaviorId BehaviorId { get; set; }

        public string NotificationName { get; set; }
    }
}
