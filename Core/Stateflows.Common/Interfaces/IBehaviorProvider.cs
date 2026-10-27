using System.Collections.Generic;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Provides behavior handles for a set of behavior classes.</summary>
    public interface IBehaviorProvider
    {
        /// <summary>Gets whether this provider represents behavior instances hosted by the local runtime.</summary>
        bool IsLocal { get; }

        /// <summary>Attempts to provide a handle for a behavior instance.</summary>
        /// <param name="id">The identifier of the requested behavior.</param>
        /// <param name="behavior">Receives the handle when this provider can provide the instance.</param>
        /// <returns><see langword="true"/> when this provider can provide the requested behavior.</returns>
        bool TryProvideBehavior(BehaviorId id, out IBehavior behavior);

        /// <summary>Gets behavior classes made available by this provider.</summary>
        IEnumerable<BehaviorClass> BehaviorClasses { get; }

        /// <summary>Raised when the set of behavior classes changes.</summary>
        event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;
    }
}
