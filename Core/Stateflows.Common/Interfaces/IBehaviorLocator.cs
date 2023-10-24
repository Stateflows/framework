using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IBehaviorLocator
    {
        IEnumerable<IBehaviorProvider> Providers { get; }

        bool TryLocateBehavior(BehaviorId id, out IBehavior behavior);
    }
}
