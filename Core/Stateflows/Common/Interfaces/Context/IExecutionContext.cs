using System.Collections.Generic;

namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        object ExecutionTrigger { get; }

        List<EventHeader> Headers { get; }
    }
}
