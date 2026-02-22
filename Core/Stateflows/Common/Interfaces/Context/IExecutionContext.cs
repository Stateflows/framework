using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        object ExecutionTrigger { get; }

        Dictionary<string, EventHeader> Headers { get; }
        
        CancellationToken CancellationToken { get; }
    }
}
