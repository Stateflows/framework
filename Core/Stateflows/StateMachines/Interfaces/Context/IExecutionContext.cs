using System.Collections.Generic;

namespace Stateflows.StateMachines
{
    /// <summary>Extends behavior execution information with the transitions taken during this execution.</summary>
    public interface IExecutionContext : Common.IExecutionContext
    {
        /// <summary>The recorded transition steps in execution order, including completion-triggered transitions.</summary>
        IEnumerable<IExecutionStep> ExecutionSteps { get; }
    }
}
