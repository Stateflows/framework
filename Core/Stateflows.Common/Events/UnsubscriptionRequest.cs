namespace Stateflows.Common
{
    public sealed class UnsubscriptionRequest : Request<UnsubscriptionResponse>
    {
        public BehaviorId BehaviorId { get; set; }

        public string EventName { get; set; }
    }
}
