namespace Stateflows.Common
{
    [NoImplicitInitialization]
    internal class SetContextOwner : SystemEvent
    {
        public BehaviorId ContextOwner { get; set; }
    }
}
