namespace Stateflows.Common
{
    [DoNotTrace]
    public sealed class BehaviorStatusRequest : Request<BehaviorStatusNotification>
    {
        public override string Name => nameof(BehaviorStatusRequest);
    }
}
