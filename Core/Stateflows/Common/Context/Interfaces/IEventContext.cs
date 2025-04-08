using System;
using System.Collections.Generic;

namespace Stateflows.Common.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IBehaviorActionContext
    {
        TEvent Event { get; }

        Guid EventId { get; }
    }
}
