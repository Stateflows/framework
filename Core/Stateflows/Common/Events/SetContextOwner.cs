namespace Stateflows.Common
{
    [NoImplicitInitialization, NoTracing]
    internal class SetContextOwner : SystemEvent
    {
        public BehaviorId ContextOwnerId { get; set; }
        public BehaviorId ContextParentId { get; set; }
    }
}
