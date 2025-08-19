namespace Stateflows.Common
{
    [NoImplicitInitialization, NoTracing]
    internal class SetContextOwner : SystemEvent
    {
        public BehaviorId ContextOwner { get; set; }
    }
}
