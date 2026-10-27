using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Exposes definition-version information for provided behavior classes.</summary>
    public interface IVersionedBehaviorProvider
    {
        /// <summary>
        /// Definition versions registered for each provided behavior class.
        /// </summary>
        IReadOnlyDictionary<BehaviorClass, int[]> BehaviorVersions { get; }
    }
}
