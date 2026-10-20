using System.Diagnostics;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Extensions.OpenTelemetry.Headers
{
    public class ActivityHeader : EventHeader
    {
        // ActivityTraceId and ActivitySpanId expose no serializable members, so the context
        // is carried in W3C Trace Context form to survive serialized transports (e.g. Orleans).
        public string? TraceParent { get; set; }

        public string? TraceState { get; set; }

        private ActivityContext? localContext;

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
