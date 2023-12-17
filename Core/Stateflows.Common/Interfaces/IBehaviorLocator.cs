using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IBehaviorLocator
    {
        bool TryLocateBehavior(BehaviorId id, out IBehavior behavior);
    }
}
