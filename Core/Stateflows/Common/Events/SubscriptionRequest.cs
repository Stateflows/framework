namespace Stateflows.Common.Events
{
    public sealed class SubscriptionRequest : Request<SubscriptionResponse>
    {
        public Context.Subscription Subscription { get; set; }
    }
}
