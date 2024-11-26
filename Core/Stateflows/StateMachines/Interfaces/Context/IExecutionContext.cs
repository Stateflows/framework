using System.Collections.Generic;

namespace Stateflows.StateMachines
{
    public interface IExecutionContext : Common.IExecutionContext
    {
        IEnumerable<IExecutionStep> ExecutionSteps { get; }
    }
}
