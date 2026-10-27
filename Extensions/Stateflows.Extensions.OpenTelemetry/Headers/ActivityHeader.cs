using System.Diagnostics;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Extensions.OpenTelemetry.Headers
{
    /// <summary>Carries W3C trace context in event headers across Stateflows dispatch and serialized transports.</summary>
    public class ActivityHeader : EventHeader
    {
        // ActivityTraceId and ActivitySpanId expose no serializable members, so the context
        // is carried in W3C Trace Context form to survive serialized transports (e.g. Orleans).
        /// <summary>Gets or sets the W3C <c>traceparent</c> value used to restore a remote trace context.</summary>
        public string? TraceParent { get; set; }

        /// <summary>Gets or sets the W3C <c>tracestate</c> value associated with <see cref="TraceParent"/>.</summary>
        public string? TraceState { get; set; }

        private ActivityContext? localContext;

        /// <summary>Gets or sets the parsed activity context; setting it updates the W3C header fields.</summary>
        [JsonIgnore]
        public ActivityContext ActivityContext
        {
            get => localContext ?? (TraceParent != null &&
                ActivityContext.TryParse(TraceParent, TraceState, isRemote: true, out var context)
                    ? context
                    : default);
            set
            {
                localContext = value;
                if (value == default)
                {
                    TraceParent = null;
                    TraceState = null;
                    return;
                }

                TraceParent = $"00-{value.TraceId.ToHexString()}-{value.SpanId.ToHexString()}-{(byte)value.TraceFlags:x2}";
                TraceState = value.TraceState;
            }
        }
    }
}
