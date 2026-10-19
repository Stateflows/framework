using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IVersionedBehaviorProvider
    {
        /// <summary>
        /// Definition versions registered for each provided behavior class.
        /// </summary>
        IReadOnlyDictionary<BehaviorClass, int[]> BehaviorVersions { get; }
    }
}
