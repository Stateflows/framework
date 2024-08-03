using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{

    public interface IExecutionContext
    {
        Event ExecutionTrigger { get; }
    }
}
