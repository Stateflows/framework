namespace Stateflows.Common.Interfaces
{
    /// <summary>Inspects registered behavior class definitions.</summary>
    public interface IBehaviorClassInspector
    {
        /// <summary>Gets event-type information for a behavior class.</summary>
        /// <param name="behaviorClass">The class to inspect.</param>
        /// <returns>The inspection result, or <see langword="null"/> when the class is not registered.</returns>
        IBehaviorClassInspection Inspect(BehaviorClass behaviorClass);
    }
}
