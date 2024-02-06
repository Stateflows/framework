namespace Stateflows.Common
{
    [DoNotTrace]
    public sealed class BehaviorStatusRequest : Request<BehaviorStatusResponse>
    {
        public override string Name => nameof(BehaviorStatusRequest);
    }
}
