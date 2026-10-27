using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Exposes behavior classes available from local and remote behavior providers.</summary>
    public interface IBehaviorClassesProvider
    {
        /// <summary>Gets behavior classes hosted by the local runtime.</summary>
        IEnumerable<BehaviorClass> LocalBehaviorClasses { get; }
        /// <summary>Gets behavior classes available from remote providers.</summary>
        IEnumerable<BehaviorClass> RemoteBehaviorClasses { get; }
        /// <summary>Gets the combined set of local and remote behavior classes.</summary>
        IEnumerable<BehaviorClass> AllBehaviorClasses { get; }
    }
}
