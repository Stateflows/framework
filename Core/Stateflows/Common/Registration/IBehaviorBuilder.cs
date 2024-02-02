namespace Stateflows.Common.Registration
{
    public interface IBehaviorBuilder
    {
        BehaviorClass BehaviorClass { get; }

        int BehaviorVersion { get; }
    }
}