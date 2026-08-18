using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        object ExecutionTrigger { get; }
        
        Guid ExecutionTriggerId { get; }

        IDictionary<string, EventHeader> Headers { get; }
        
        CancellationToken CancellationToken { get; }
    }
}
