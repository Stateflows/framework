using System;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        object ExecutionTrigger { get; }
        public Guid ExecutionTriggerId { get; }
        public IEnumerable<EventHeader> Headers { get; }
    }
}
