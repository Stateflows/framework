namespace Stateflows.Common
{
    public class BehaviorStatusResponse : Response
    {
        public override string EventName => nameof(BehaviorStatusResponse);

        public BehaviorStatus BehaviorStatus { get; set; }
    }
}
