namespace Stateflows.Common
{
    public interface IBehaviorLocator
    {
        bool TryLocateBehavior(BehaviorId id, out IBehavior behavior);
    }
}
