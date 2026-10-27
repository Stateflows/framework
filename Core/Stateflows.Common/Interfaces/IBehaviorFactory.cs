namespace Stateflows.Common.Interfaces;

/// <summary>Creates client handles for behavior instances.</summary>
public interface IBehaviorFactory
{
    /// <summary>Creates a behavior handle for an identifier.</summary>
    /// <param name="behaviorId">The identifier of the target behavior.</param>
    /// <returns>A handle bound to the specified behavior identifier.</returns>
    IBehavior CreateBehavior(BehaviorId behaviorId);
}