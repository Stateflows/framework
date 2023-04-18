using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IBehaviorClassesProvider
    {
        IEnumerable<BehaviorClass> LocalBehaviorClasses { get; }
        IEnumerable<BehaviorClass> RemoteBehaviorClasses { get; }
        IEnumerable<BehaviorClass> AllBehaviorClasses { get; }
    }
}
