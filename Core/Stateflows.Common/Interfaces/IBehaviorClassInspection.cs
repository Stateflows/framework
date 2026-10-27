using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Describes a behavior class and the event types it accepts.</summary>
    public interface IBehaviorClassInspection
    {
        /// <summary>Gets the inspected behavior class.</summary>
        BehaviorClass BehaviorClass { get; }
        /// <summary>Gets the event types that can initialize the behavior.</summary>
        IEnumerable<Type> InitializationEventTypes { get; }
        /// <summary>Gets the event types handled after initialization.</summary>
        IEnumerable<Type> EventTypes { get; }
    }
}
