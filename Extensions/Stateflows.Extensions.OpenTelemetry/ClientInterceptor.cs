using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    /// <summary>Propagates the current diagnostic activity as a trace header on dispatched events.</summary>
    public class ClientInterceptor : IStateflowsClientInterceptor
    {
        /// <summary>Attaches the current activity context to an outgoing event before continuing dispatch.</summary>
        /// <param name="targetBehaviorId">The intended recipient behavior.</param>
        /// <param name="event">The event payload.</param>
        /// <param name="headers">The event's mutable delivery headers.</param>
        /// <param name="next">The remaining interceptors and dispatch.</param>
        public async Task DispatchEventAsync<TEvent>(BehaviorId targetBehaviorId, TEvent @event,
            IDictionary<string, EventHeader> headers, Func<Task> next)
        {
            if (Activity.Current != null)
            {
                headers[nameof(ActivityHeader)] = new ActivityHeader { ActivityContext = Activity.Current.Context };
            }

            await next();
        }
    }
}