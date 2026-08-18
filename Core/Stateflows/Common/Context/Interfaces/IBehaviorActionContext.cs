namespace Stateflows.Common
{
    public interface IBehaviorActionContext : IBehaviorLocator, IExecutionContext
    {
        IBehaviorContext Behavior { get; }
        
        // IParentBehaviorContext? ParentBehavior { get; }
        
        bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext);
        
        // IOwnerBehaviorContext? OwnerBehavior { get; }
        
        bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext);
    }
}
