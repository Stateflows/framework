namespace Stateflows.Common
{
    public interface IBehaviorLocator
    {
        /// <summary>
        /// Locates handle of behavior with given identifier.
        /// </summary>
        /// <param name="id">Identifier of behavior</param>
        /// <param name="behavior">out parameter providing behavior handle</param>
        /// <returns>True if behavior handle is available; false otherwise</returns>
        bool TryLocateBehavior(BehaviorId id, out IBehavior behavior);
    }
}
