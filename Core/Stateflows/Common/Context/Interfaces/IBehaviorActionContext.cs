namespace Stateflows.Common
{
    public interface IBehaviorActionContext : IBehaviorLocator, IExecutionContext
    {
        IBehaviorContext Behavior { get; }
        
        bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext);
        
        bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext);
    }
}
