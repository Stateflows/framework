namespace Stateflows.Common.Registration
{
    /// <summary>Exposes the identity and version of a behavior being registered.</summary>
    public interface IBehaviorBuilder
    {
        /// <summary>Gets the behavior class represented by this builder.</summary>
        BehaviorClass BehaviorClass { get; }

        /// <summary>Gets the version of the behavior definition represented by this builder.</summary>
        int BehaviorVersion { get; }
    }
}