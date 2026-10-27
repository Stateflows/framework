using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    /// <summary>Propagates the current diagnostic activity as a trace header on dispatched events.</summary>
    public class ClientInterceptor : IStateflowsClientInterceptor
    {
        /// <summary>Attaches the current activity context to the outgoing event when one exists.</summary>
        /// <param name="eventHolder">The event about to be dispatched.</param>
        /// <returns><see langword="true"/> so dispatch proceeds.</returns>
        public bool BeforeDispatchEvent(EventHolder eventHolder)
        {
            if (Activity.Current != null)
            {
                eventHolder.Headers[nameof(ActivityHeader)] = new ActivityHeader { ActivityContext = Activity.Current.Context };
            }

            return true;
        }

        /// <summary>Completes the client-dispatch interception stage.</summary>
        /// <param name="eventHolder">The dispatched event.</param>
        public void AfterDispatchEvent(EventHolder eventHolder)
        { }
    }
}