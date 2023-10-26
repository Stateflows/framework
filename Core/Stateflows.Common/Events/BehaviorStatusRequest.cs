namespace Stateflows.Common
{
    public sealed class BehaviorStatusRequest : Request<BehaviorStatusResponse>
    {
        public override string EventName => nameof(BehaviorStatusRequest);
    }
}
