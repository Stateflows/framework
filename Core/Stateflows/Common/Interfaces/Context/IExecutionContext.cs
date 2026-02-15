using System.Collections.Generic;

namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        object ExecutionTrigger { get; }

        Dictionary<string, EventHeader> Headers { get; }
    }
}
