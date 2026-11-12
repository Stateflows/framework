using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common
{
    /// <summary>Exposes invocation mode, optional event metadata, and cancellation.</summary>
    public interface IExecutionContext
    {
        /// <summary>Gets the current invocation's execution ownership, not the Activity's overall busy mode.</summary>
        ExecutionMode ExecutionMode { get; }

        /// <summary>Gets the triggering event payload, or null for streamed/token-enabled execution.</summary>
        object? ExecutionTrigger { get; }
        
        /// <summary>Gets the triggering delivery ID, or null when no event trigger is associated with this invocation.</summary>
        Guid? ExecutionTriggerId { get; }

        /// <summary>Gets triggering event headers, or null when no event trigger is associated with this invocation.</summary>
        IDictionary<string, EventHeader>? Headers { get; }
        
        /// <summary>Gets the cancellation token for the current execution.</summary>
        CancellationToken CancellationToken { get; }
    }
}
