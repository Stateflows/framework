using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IBehaviorClassInspection
    {
        BehaviorClass BehaviorClass { get; }
        IEnumerable<Type> InitializationEventTypes { get; }
        IEnumerable<Type> EventTypes { get; }
    }
}
