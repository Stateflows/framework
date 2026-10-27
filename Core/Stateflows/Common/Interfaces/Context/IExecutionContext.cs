using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common
{
    /// <summary>Exposes the current behavior execution's trigger, headers, and cancellation token.</summary>
    public interface IExecutionContext
    {
        /// <summary>Gets the event payload that triggered the current execution.</summary>
        object ExecutionTrigger { get; }
        
        /// <summary>Gets the identifier of the triggering event delivery.</summary>
        Guid ExecutionTriggerId { get; }

        /// <summary>Gets the headers attached to the triggering event.</summary>
        IDictionary<string, EventHeader> Headers { get; }
        
        /// <summary>Gets the cancellation token for the current execution.</summary>
        CancellationToken CancellationToken { get; }
    }
}
