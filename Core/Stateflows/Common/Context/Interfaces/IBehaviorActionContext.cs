namespace Stateflows.Common
{
    /// <summary>Exposes the behavior, execution trigger, related behavior contexts, and locator operations to a behavior action.</summary>
    public interface IBehaviorActionContext : IBehaviorLocator, IExecutionContext
    {
        /// <summary>Gets the context of the behavior currently executing an action.</summary>
        IBehaviorContext Behavior { get; }
        
        /// <summary>Attempts to get the containing parent behavior context.</summary>
        /// <param name="parentBehaviorContext">Receives the parent context when one exists.</param>
        /// <returns><see langword="true"/> when this behavior has a parent.</returns>
        bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext);
        
        /// <summary>Attempts to get the owning behavior context.</summary>
        /// <param name="ownerBehaviorContext">Receives the owner context when one exists.</param>
        /// <returns><see langword="true"/> when this behavior has an owner.</returns>
        bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext);
    }
}
