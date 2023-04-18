using System.Collections.Generic;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Interfaces
{
    public interface IBehaviorProvider
    {
        bool IsLocal { get; }

        bool TryProvideBehavior(BehaviorId id, out IBehavior behavior);

        IEnumerable<BehaviorClass> BehaviorClasses { get; }

        event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;
    }
}
