namespace Stateflows.Common.Interfaces;

public interface IBehaviorFactory
{
    IBehavior CreateBehavior(BehaviorId behaviorId);
}