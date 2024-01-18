namespace Stateflows.Common
{
    public class BehaviorStatusResponse : Response
    {
        public override string Name => nameof(BehaviorStatusResponse);

        public BehaviorStatus BehaviorStatus { get; set; }
    }
}
